using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mwanzo.Data;
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

        public TeachersController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher([FromBody] Teacher teacher)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(teacher.UserId);
            if (user == null || user.Role != UserRole.Teacher) return BadRequest("Invalid user or role");

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Teacher", teacher.Id.ToString());
            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacher);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Subject)
                .ToListAsync();
            return Ok(teachers);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.SubjectAssignments)
                    .ThenInclude(sa => sa.Subject)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null) return NotFound();
            return Ok(teacher);
        }

        [HttpPost("assign-subject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubject([FromBody] SubjectAssignment assignment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Business rule: One teacher per subject per class
            var existing = await _context.SubjectAssignments
                .AnyAsync(sa => sa.SubjectId == assignment.SubjectId && sa.ClassId == assignment.ClassId);
            if (existing) return BadRequest("Subject already assigned to a teacher for this class");

            _context.SubjectAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Assigned", "SubjectAssignment", assignment.Id.ToString());
            return Ok(assignment);
        }
    }
}