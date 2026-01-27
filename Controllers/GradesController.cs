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
            if (dto.Marks < 0 || dto.Marks > 100) return BadRequest("Marks must be 0-100");

            var grade = _mapper.Map<Grade>(dto);
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Grade", grade.Id.ToString());

            return CreatedAtAction(nameof(GetGrade), new { id = grade.Id }, _mapper.Map<GradeResponseDto>(grade));
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
            var grades = await _context.Grades
                .Include(g => g.Exam)
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            if (!grades.Any()) return NotFound();

            var average = grades.Average(g => g.Marks);
            return Ok(new { StudentId = studentId, Grades = _mapper.Map<List<GradeResponseDto>>(grades), AverageMarks = average });
        }
    }
}
