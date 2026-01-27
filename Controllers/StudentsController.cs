using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mwanzo.Data;
using mwanzo.Models;
using mwanzo.Services;
using System.Linq;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public StudentsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate user exists and is a student
            var user = await _context.Users.FindAsync(student.UserId);
            if (user == null || user.Role != UserRole.Student) return BadRequest("Invalid user or role");

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Student", student.Id.ToString(), $"Student {student.Id} created");
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetStudents(int? classId, string? status, int page = 1, int pageSize = 10)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .AsQueryable();

            if (classId.HasValue) query = query.Where(s => s.ClassId == classId);
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<UserStatus>(status, out var userStatus))
                    query = query.Where(s => s.User.Status == userStatus);
            }

            var total = await query.CountAsync();
            var students = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { Total = total, Page = page, PageSize = pageSize, Data = students });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .Include(s => s.Attendances)
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student updatedStudent)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.ClassId = updatedStudent.ClassId;
            student.EnrollmentDate = updatedStudent.EnrollmentDate;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Student", id.ToString(), $"Student {id} updated");
            return Ok(student);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Student", id.ToString(), $"Student {id} deleted");
            return NoContent();
        }
    }
}