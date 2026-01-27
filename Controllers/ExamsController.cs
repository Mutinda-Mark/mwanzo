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
    public class ExamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ExamsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] Exam exam)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Exam", exam.Id.ToString());
            return Ok(exam);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetExams(int? classId)
        {
            var query = _context.Exams.Include(e => e.Subject).AsQueryable();
            if (classId.HasValue) query = query.Where(e => e.ClassId == classId);
            var exams = await query.ToListAsync();
            return Ok(exams);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] Exam updatedExam)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            exam.Name = updatedExam.Name;
            exam.ExamDate = updatedExam.ExamDate;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Exam", id.ToString(), $"Exam {id} updated");
            return Ok(exam);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Exam", id.ToString(), $"Exam {id} deleted");
            return NoContent();
        }
    }
}