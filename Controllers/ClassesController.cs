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
    public class ClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;

        public ClassesController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            var @class = _mapper.Map<Class>(dto);
            _context.Classes.Add(@class);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Class", @class.Id.ToString());

            return CreatedAtAction(nameof(GetClass), new { id = @class.Id }, _mapper.Map<ClassResponseDto>(@class));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.TimetableEntries)
                .ToListAsync();

            return Ok(_mapper.Map<List<ClassResponseDto>>(classes));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClass(int id)
        {
            var @class = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.TimetableEntries)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@class == null) return NotFound();
            return Ok(_mapper.Map<ClassResponseDto>(@class));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassUpdateDto dto)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            _mapper.Map(dto, @class);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Class", id.ToString());

            return Ok(_mapper.Map<ClassResponseDto>(@class));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            _context.Classes.Remove(@class);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Class", id.ToString());

            return NoContent();
        }
    }
}
