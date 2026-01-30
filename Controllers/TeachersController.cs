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

                var teacher = new Teacher { UserId = dto.UserId };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                var response = await _context.Teachers
                    .Include(t => t.User)
                    .Include(t => t.SubjectAssignments)
                        .ThenInclude(sa => sa.Class)
                    .Include(t => t.SubjectAssignments)
                        .ThenInclude(sa => sa.Subject)
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
                    .Include(t => t.SubjectAssignments)
                        .ThenInclude(sa => sa.Class)
                    .Include(t => t.SubjectAssignments)
                        .ThenInclude(sa => sa.Subject)
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

            var assigned = new List<SubjectAssignmentResponseDto>();
            var skipped = new List<SubjectAssignmentCreateDto>();

            try
            {
                foreach (var dto in dtos)
                {
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == dto.TeacherId);
                    if (teacher == null || !await _context.Classes.AnyAsync(c => c.Id == dto.ClassId) || 
                        !await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                    {
                        skipped.Add(dto);
                        continue;
                    }

                    // Avoid duplicates
                    bool exists = await _context.SubjectAssignments
                        .AnyAsync(sa => sa.SubjectId == dto.SubjectId && sa.ClassId == dto.ClassId);

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
                    assigned.Add(_mapper.Map<SubjectAssignmentResponseDto>(assignment));
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    AssignedCount = assigned.Count,
                    SkippedCount = skipped.Count,
                    Assigned = assigned,
                    Skipped = skipped
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning subjects to teachers");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
