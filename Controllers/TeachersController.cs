using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using mwanzo.Data;
using mwanzo.DTOs;
using mwanzo.Models;
using mwanzo.Services;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;

        public TeachersController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null || user.Role != UserRole.Teacher) return BadRequest("Invalid user or role");

            var teacher = _mapper.Map<Teacher>(dto);
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Teacher", teacher.Id.ToString());

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, _mapper.Map<TeacherResponseDto>(teacher));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Subject)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Class)
                .ToListAsync();

            return Ok(_mapper.Map<List<TeacherResponseDto>>(teachers));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Subject)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Class)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null) return NotFound();
            return Ok(_mapper.Map<TeacherResponseDto>(teacher));
        }

        [HttpPost("assign-subject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubject([FromBody] SubjectAssignmentCreateDto dto)
        {
            var exists = await _context.SubjectAssignments
                .AnyAsync(sa => sa.SubjectId == dto.SubjectId && sa.ClassId == dto.ClassId);

            if (exists) return BadRequest("Subject already assigned for this class");

            var assignment = _mapper.Map<SubjectAssignment>(dto);
            _context.SubjectAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Assigned", "SubjectAssignment", assignment.Id.ToString());

            return Ok(_mapper.Map<SubjectAssignmentResponseDto>(assignment));
        }
    }
}
