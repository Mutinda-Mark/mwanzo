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
    public class TimetableController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;

        public TimetableController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTimetable([FromBody] TimetableCreateDto dto)
        {
            var conflict = await _context.TimetableEntries
                .AnyAsync(te => te.ClassId == dto.ClassId && te.Day == dto.Day &&
                                dto.StartTime < te.EndTime && dto.EndTime > te.StartTime);
            if (conflict) return BadRequest("Timetable conflict detected");

            var entry = _mapper.Map<TimetableEntry>(dto);
            _context.TimetableEntries.Add(entry);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "TimetableEntry", entry.Id.ToString());

            return Ok(_mapper.Map<TimetableResponseDto>(entry));
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetTimetable(int classId)
        {
            var entries = await _context.TimetableEntries
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Where(te => te.ClassId == classId)
                .ToListAsync();

            return Ok(_mapper.Map<List<TimetableResponseDto>>(entries));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTimetable(int id, [FromBody] TimetableCreateDto dto)
        {
            var entry = await _context.TimetableEntries.FindAsync(id);
            if (entry == null) return NotFound();

            var conflict = await _context.TimetableEntries
                .Where(te => te.Id != id && te.ClassId == dto.ClassId && te.Day == dto.Day)
                .AnyAsync(te => dto.StartTime < te.EndTime && dto.EndTime > te.StartTime);
            if (conflict) return BadRequest("Timetable conflict detected");

            _mapper.Map(dto, entry);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "TimetableEntry", id.ToString());

            return Ok(_mapper.Map<TimetableResponseDto>(entry));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTimetable(int id)
        {
            var entry = await _context.TimetableEntries.FindAsync(id);
            if (entry == null) return NotFound();

            _context.TimetableEntries.Remove(entry);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "TimetableEntry", id.ToString());

            return NoContent();
        }
    }
}
