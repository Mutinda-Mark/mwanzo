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

        public TeachersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Create a teacher
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return BadRequest("Invalid user");

            var teacher = new Teacher
            {
                UserId = dto.UserId
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<TeacherResponseDto>(teacher);
            return Ok(response);
        }

        // Get all teachers
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Class)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Subject)
                .ToListAsync();

            var response = _mapper.Map<List<TeacherResponseDto>>(teachers);
            return Ok(response);
        }

        // Assign subject to teacher
        // Assign subjects to teachers - improved
        [HttpPost("assign-subject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubjects([FromBody] List<SubjectAssignmentCreateDto> dtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var assigned = new List<SubjectAssignmentResponseDto>();
            var skipped = new List<SubjectAssignmentCreateDto>();

            foreach (var dto in dtos)
            {
                // Find teacher
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == dto.TeacherId);
                if (teacher == null)
                {
                    skipped.Add(dto);
                    continue; // skip invalid teacher
                }

                // Check class and subject exist
                var cls = await _context.Classes.FindAsync(dto.ClassId);
                var subject = await _context.Subjects.FindAsync(dto.SubjectId);
                if (cls == null || subject == null)
                {
                    skipped.Add(dto);
                    continue; // skip invalid class/subject
                }

                // Check if this Subject+Class already exists
                bool exists = await _context.SubjectAssignments
                    .AnyAsync(sa => sa.SubjectId == dto.SubjectId && sa.ClassId == dto.ClassId);

                if (exists)
                {
                    skipped.Add(dto); // duplicate, skip
                    continue;
                }

                // Create assignment
                var assignment = new SubjectAssignment
                {
                    TeacherId = teacher.Id,
                    SubjectId = dto.SubjectId,
                    ClassId = dto.ClassId
                };

                _context.SubjectAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                assigned.Add(_mapper.Map<SubjectAssignmentResponseDto>(assignment));
            }

            // Return summary
            return Ok(new
            {
                AssignedCount = assigned.Count,
                SkippedCount = skipped.Count,
                Assigned = assigned,
                Skipped = skipped
            });
        }



    }
}
