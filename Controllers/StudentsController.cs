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
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;

        public StudentsController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null || user.Role != UserRole.Student) return BadRequest("Invalid user or role");

            var student = _mapper.Map<Student>(dto);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Student", student.Id.ToString());

            var response = _mapper.Map<StudentResponseDto>(student);
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();
            return Ok(_mapper.Map<StudentResponseDto>(student));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentUpdateDto dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            _mapper.Map(dto, student);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Student", id.ToString());

            return Ok(_mapper.Map<StudentResponseDto>(student));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Student", id.ToString());

            return NoContent();
        }
    }
}
