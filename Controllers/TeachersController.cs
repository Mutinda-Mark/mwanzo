using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mwanzo.Data;
using mwanzo.DTOs;
using mwanzo.Models;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(ApplicationDbContext context, IMapper mapper, ILogger<TeachersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // Create a teacher
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null) return BadRequest(new { message = "Invalid user ID" });

                // OPTIONAL safety check: prevent duplicate teacher rows for same userId
                bool alreadyTeacher = await _context.Teachers.AnyAsync(t => t.UserId == dto.UserId);
                if (alreadyTeacher) return BadRequest(new { message = "A teacher already exists for this user ID." });

                var teacher = new Teacher { UserId = dto.UserId };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                var response = await _context.Teachers
                    .Include(t => t.User)
                    .Include(t => t.SubjectAssignments).ThenInclude(sa => sa.Class)
                    .Include(t => t.SubjectAssignments).ThenInclude(sa => sa.Subject)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == teacher.Id);

                return Ok(_mapper.Map<TeacherResponseDto>(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // Get all teachers
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeachers()
        {
            try
            {
                var teachers = await _context.Teachers
                    .Include(t => t.User)
                    .Include(t => t.SubjectAssignments).ThenInclude(sa => sa.Class)
                    .Include(t => t.SubjectAssignments).ThenInclude(sa => sa.Subject)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(_mapper.Map<List<TeacherResponseDto>>(teachers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teachers");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // Assign subjects to teachers
        [HttpPost("assign-subject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubjects([FromBody] List<SubjectAssignmentCreateDto> dtos)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdAssignments = new List<SubjectAssignment>();
            var skipped = new List<SubjectAssignmentCreateDto>();

            try
            {
                foreach (var dto in dtos)
                {
                    // TeacherId in DTO is the GUID UserId (AspNetUsers.Id)
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == dto.TeacherId);
                    if (teacher == null)
                    {
                        skipped.Add(dto);
                        continue;
                    }

                    bool classExists = await _context.Classes.AnyAsync(c => c.Id == dto.ClassId);
                    bool subjectExists = await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);

                    if (!classExists || !subjectExists)
                    {
                        skipped.Add(dto);
                        continue;
                    }

                    // Avoid duplicates PER TEACHER
                    bool exists = await _context.SubjectAssignments.AnyAsync(sa =>
                        sa.TeacherId == teacher.Id &&
                        sa.SubjectId == dto.SubjectId &&
                        sa.ClassId == dto.ClassId);

                    if (exists)
                    {
                        skipped.Add(dto);
                        continue;
                    }

                    var assignment = new SubjectAssignment
                    {
                        TeacherId = teacher.Id,
                        SubjectId = dto.SubjectId,
                        ClassId = dto.ClassId
                    };

                    _context.SubjectAssignments.Add(assignment);
                    createdAssignments.Add(assignment);
                }

                await _context.SaveChangesAsync();

                // Reload created assignments with Subject + Class to populate SubjectName/ClassName + Ids
                var createdIds = createdAssignments.Select(a => a.Id).ToList();

                var reloaded = await _context.SubjectAssignments
                    .AsNoTracking()
                    .Where(sa => createdIds.Contains(sa.Id))
                    .Include(sa => sa.Subject)
                    .Include(sa => sa.Class)
                    .ToListAsync();

                var assignedDtos = _mapper.Map<List<SubjectAssignmentResponseDto>>(reloaded);

                return Ok(new
                {
                    AssignedCount = assignedDtos.Count,
                    SkippedCount = skipped.Count,
                    Assigned = assignedDtos,
                    Skipped = skipped
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning subjects to teachers");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // UPDATE an assignment (Admin only)
        [HttpPut("assign-subject/{assignmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAssignment(int assignmentId, [FromBody] SubjectAssignmentUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var assignment = await _context.SubjectAssignments.FindAsync(assignmentId);
                if (assignment == null) return NotFound(new { message = "Assignment not found." });

                bool classExists = await _context.Classes.AnyAsync(c => c.Id == dto.ClassId);
                bool subjectExists = await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
                if (!classExists || !subjectExists)
                    return BadRequest(new { message = "Invalid class or subject." });

                // Avoid duplicates PER TEACHER
                bool exists = await _context.SubjectAssignments.AnyAsync(sa =>
                    sa.Id != assignmentId &&
                    sa.TeacherId == assignment.TeacherId &&
                    sa.SubjectId == dto.SubjectId &&
                    sa.ClassId == dto.ClassId);

                if (exists) return BadRequest(new { message = "Duplicate assignment exists for this teacher." });

                assignment.ClassId = dto.ClassId;
                assignment.SubjectId = dto.SubjectId;

                await _context.SaveChangesAsync();

                // Reload with subject & class so names are included
                var reloaded = await _context.SubjectAssignments
                    .AsNoTracking()
                    .Include(sa => sa.Class)
                    .Include(sa => sa.Subject)
                    .FirstAsync(sa => sa.Id == assignmentId);

                return Ok(_mapper.Map<SubjectAssignmentResponseDto>(reloaded));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject assignment");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // DELETE an assignment (Admin only)
        [HttpDelete("assign-subject/{assignmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            try
            {
                var assignment = await _context.SubjectAssignments.FindAsync(assignmentId);
                if (assignment == null) return NotFound(new { message = "Assignment not found." });

                _context.SubjectAssignments.Remove(assignment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject assignment");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
