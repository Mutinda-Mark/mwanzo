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
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<GradesController> _logger;

        public GradesController(ApplicationDbContext context, AuditService auditService, IMapper mapper, ILogger<GradesController> logger)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateGrade([FromBody] GradeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Validate student
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == dto.StudentId);
                if (student == null)
                    return BadRequest(new { message = "Invalid student ID." });

                // Validate exam
                var exam = await _context.Exams.FindAsync(dto.ExamId);
                if (exam == null)
                    return BadRequest(new { message = "Invalid exam ID." });

                var grade = _mapper.Map<Grade>(dto);
                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Created", "Grade", grade.Id.ToString());

                // Reload grade with related entities
                grade = await _context.Grades
                    .AsNoTracking()
                    .Include(g => g.Student).ThenInclude(s => s.User)
                    .Include(g => g.Exam)
                    .FirstOrDefaultAsync(g => g.Id == grade.Id);

                var response = new GradeResponseDto
                {
                    Id = grade.Id,
                    StudentId = grade.StudentId,
                    StudentName = $"{grade.Student.User.FirstName} {grade.Student.User.LastName}",
                    ExamId = grade.ExamId,
                    ExamName = grade.Exam.Name,
                    Marks = grade.Marks,
                    Comments = grade.Comments
                };

                return CreatedAtAction(nameof(GetGrade), new { id = grade.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating grade");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrade(int id)
        {
            try
            {
                var grade = await _context.Grades
                    .AsNoTracking()
                    .Include(g => g.Student).ThenInclude(s => s.User)
                    .Include(g => g.Exam).ThenInclude(e => e.Subject)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (grade == null)
                    return NotFound(new { message = "Grade not found." });

                return Ok(_mapper.Map<GradeResponseDto>(grade));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching grade {GradeId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] GradeUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var grade = await _context.Grades.FindAsync(id);
                if (grade == null)
                    return NotFound(new { message = "Grade not found." });

                _mapper.Map(dto, grade);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Updated", "Grade", id.ToString());

                return Ok(_mapper.Map<GradeResponseDto>(grade));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating grade {GradeId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            try
            {
                var grade = await _context.Grades.FindAsync(id);
                if (grade == null)
                    return NotFound(new { message = "Grade not found." });

                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Deleted", "Grade", id.ToString());

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting grade {GradeId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("report/{studentId}")]
        public async Task<IActionResult> GetStudentReport(int studentId)
        {
            try
            {
                var grades = await _context.Grades
                    .AsNoTracking()
                    .Include(g => g.Exam)
                    .Include(g => g.Student).ThenInclude(s => s.User)
                    .Where(g => g.StudentId == studentId)
                    .ToListAsync();

                if (!grades.Any())
                    return NotFound(new { message = "No grades found for this student." });

                var studentUser = grades.First().Student?.User;
                var studentName = studentUser != null
                    ? $"{studentUser.FirstName} {studentUser.LastName}"
                    : "Unknown Student";

                var gradeDtos = grades.Select(g => new GradeResponseDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = studentName,
                    ExamId = g.ExamId,
                    ExamName = g.Exam.Name,
                    Marks = g.Marks,
                    Comments = g.Comments
                }).ToList();

                var average = grades.Average(g => g.Marks);

                return Ok(new
                {
                    StudentId = studentId,
                    StudentName = studentName,
                    Grades = gradeDtos,
                    AverageMarks = average
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching grades report for student {StudentId}", studentId);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
