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
    public class ExamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(ApplicationDbContext context, AuditService auditService, IMapper mapper, ILogger<ExamsController> logger)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new exam. Admin/Teacher only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] ExamCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data." });

            try
            {
                // Optional: Check for duplicate exam for same class/subject/date
                var exists = await _context.Exams.AnyAsync(e =>
                    e.ClassId == dto.ClassId && e.SubjectId == dto.SubjectId && e.ExamDate.Date == dto.ExamDate.Date);
                if (exists)
                    return BadRequest(new { message = "An exam for this class and subject on this date already exists." });

                var exam = _mapper.Map<Exam>(dto);
                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Created", "Exam", exam.Id.ToString());

                return Ok(_mapper.Map<ExamResponseDto>(exam));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves all exams, optionally filtered by classId.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetExams(int? classId)
        {
            try
            {
                var query = _context.Exams
                    .AsNoTracking()
                    .Include(e => e.Subject)
                    .Include(e => e.Class)
                    .AsQueryable();

                if (classId.HasValue)
                    query = query.Where(e => e.ClassId == classId);

                var exams = await query.ToListAsync();
                return Ok(_mapper.Map<List<ExamResponseDto>>(exams));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exams");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Updates an existing exam. Admin/Teacher only.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] ExamCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data." });

            try
            {
                var exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                    return NotFound(new { message = "Exam not found." });

                _mapper.Map(dto, exam);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Updated", "Exam", id.ToString());

                return Ok(_mapper.Map<ExamResponseDto>(exam));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam {ExamId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Deletes an exam. Admin only.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                    return NotFound(new { message = "Exam not found." });

                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Deleted", "Exam", id.ToString());

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exam {ExamId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
