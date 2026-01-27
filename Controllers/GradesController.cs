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
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public GradesController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateGrade([FromBody] Grade grade)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate that the student and exam exist
            var studentExists = await _context.Students.AnyAsync(s => s.Id == grade.StudentId);
            var examExists = await _context.Exams.AnyAsync(e => e.Id == grade.ExamId);
            if (!studentExists || !examExists) return BadRequest("Invalid student or exam ID");

            // Business rule: Marks should be between 0 and 100 (add [Range(0, 100)] to Grade.Marks in model for auto-validation)
            if (grade.Marks < 0 || grade.Marks > 100) return BadRequest("Marks must be between 0 and 100");

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Grade", grade.Id.ToString(), $"Grade {grade.Id} created for student {grade.StudentId}");
            return CreatedAtAction(nameof(GetGrade), new { id = grade.Id }, grade);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetGrades(int? studentId, int? examId, int page = 1, int pageSize = 10)
        {
            var query = _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Exam)
                    .ThenInclude(e => e.Subject)
                .AsQueryable();

            if (studentId.HasValue) query = query.Where(g => g.StudentId == studentId);
            if (examId.HasValue) query = query.Where(g => g.ExamId == examId);

            var total = await query.CountAsync();
            var grades = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { Total = total, Page = page, PageSize = pageSize, Data = grades });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Exam)
                    .ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null) return NotFound();
            return Ok(grade);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] Grade updatedGrade)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();

            // Update fields
            grade.Marks = updatedGrade.Marks;
            grade.Comments = updatedGrade.Comments;

            // Re-validate marks
            if (grade.Marks < 0 || grade.Marks > 100) return BadRequest("Marks must be between 0 and 100");

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Grade", id.ToString(), $"Grade {id} updated");
            return Ok(grade);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Grade", id.ToString(), $"Grade {id} deleted");
            return NoContent();
        }

        // Optional: Endpoint for report cards (aggregate grades for a student)
        [HttpGet("report/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetStudentReport(int studentId)
        {
            var grades = await _context.Grades
                .Where(g => g.StudentId == studentId)
                .Include(g => g.Exam)
                    .ThenInclude(e => e.Subject)
                .ToListAsync();

            if (!grades.Any()) return NotFound("No grades found for this student");

            var average = grades.Average(g => g.Marks);
            return Ok(new
            {
                StudentId = studentId,
                Grades = grades,
                AverageMarks = average
            });
        }
    }
}