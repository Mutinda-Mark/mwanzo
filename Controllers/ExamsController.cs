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

        public ExamsController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] ExamCreateDto dto)
        {
            var exam = _mapper.Map<Exam>(dto);
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Exam", exam.Id.ToString());

            return Ok(_mapper.Map<ExamResponseDto>(exam));
        }

        [HttpGet]
        public async Task<IActionResult> GetExams(int? classId)
        {
            var query = _context.Exams.Include(e => e.Subject).Include(e => e.Class).AsQueryable();
            if (classId.HasValue) query = query.Where(e => e.ClassId == classId);

            var exams = await query.ToListAsync();
            return Ok(_mapper.Map<List<ExamResponseDto>>(exams));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] ExamCreateDto dto)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            _mapper.Map(dto, exam);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Exam", id.ToString());

            return Ok(_mapper.Map<ExamResponseDto>(exam));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Exam", id.ToString());

            return NoContent();
        }
    }
}
