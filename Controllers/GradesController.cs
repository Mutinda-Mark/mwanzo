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

        public GradesController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateGrade([FromBody] GradeCreateDto dto)
        {
            // Validate marks
            if (dto.Marks < 0 || dto.Marks > 100)
                return BadRequest("Marks must be between 0 and 100");

            // Validate student
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);
            if (student == null)
                return BadRequest("Invalid student");

            // Validate exam
            var exam = await _context.Exams.FindAsync(dto.ExamId);
            if (exam == null)
                return BadRequest("Invalid exam");

            // Map DTO to Grade entity
            var grade = _mapper.Map<Grade>(dto);

            // Add to DB
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("Created", "Grade", grade.Id.ToString());

            // Load related entities for response
            grade = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Exam)
                .FirstOrDefaultAsync(g => g.Id == grade.Id);

            // Prepare response
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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Exam).ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null) return NotFound();
            return Ok(_mapper.Map<GradeResponseDto>(grade));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] GradeUpdateDto dto)
        {
            if (dto.Marks < 0 || dto.Marks > 100) return BadRequest("Marks must be 0-100");

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();

            _mapper.Map(dto, grade);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Grade", id.ToString());

            return Ok(_mapper.Map<GradeResponseDto>(grade));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Grade", id.ToString());

            return NoContent();
        }

        [HttpGet("report/{studentId}")]
        public async Task<IActionResult> GetStudentReport(int studentId)
        {
            // Include both Exam and Student.User so we can get student name
            var grades = await _context.Grades
                .Include(g => g.Exam)
                .Include(g => g.Student)          // Include Student
                    .ThenInclude(s => s.User)     // Include User inside Student
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            if (!grades.Any()) return NotFound();

            // Get student name (assumes first grade is representative)
            var studentUser = grades.First().Student?.User;
            var studentName = studentUser != null 
                ? $"{studentUser.FirstName} {studentUser.LastName}" 
                : "Unknown Student";

            // Map grades to DTOs and include student name
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

            // Calculate average
            var average = grades.Average(g => g.Marks);

            return Ok(new
            {
                StudentId = studentId,
                StudentName = studentName,
                Grades = gradeDtos,
                AverageMarks = average
            });
        }

    }
}
