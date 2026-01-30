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
        private readonly ILogger<TimetableController> _logger;

        public TimetableController(ApplicationDbContext context, AuditService auditService, IMapper mapper, ILogger<TimetableController> logger)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTimetable([FromBody] TimetableCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.EndTime <= dto.StartTime)
                return BadRequest(new { message = "EndTime must be after StartTime." });

            try
            {
                var conflict = await _context.TimetableEntries
                    .AnyAsync(te => te.ClassId == dto.ClassId && te.Day == dto.Day &&
                                    dto.StartTime < te.EndTime && dto.EndTime > te.StartTime);
                if (conflict)
                    return BadRequest(new { message = "Timetable conflict detected for this class and day." });

                var entry = _mapper.Map<TimetableEntry>(dto);
                _context.TimetableEntries.Add(entry);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Created", "TimetableEntry", entry.Id.ToString());

                var response = await _context.TimetableEntries
                    .AsNoTracking()
                    .Include(te => te.Class)
                    .Include(te => te.Subject)
                    .FirstOrDefaultAsync(te => te.Id == entry.Id);

                return Ok(_mapper.Map<TimetableResponseDto>(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timetable entry");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetTimetable(int classId)
        {
            try
            {
                var entries = await _context.TimetableEntries
                    .AsNoTracking()
                    .Include(te => te.Class)
                    .Include(te => te.Subject)
                    .Where(te => te.ClassId == classId)
                    .ToListAsync();

                return Ok(_mapper.Map<List<TimetableResponseDto>>(entries));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching timetable for class {ClassId}", classId);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTimetable(int id, [FromBody] TimetableCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.EndTime <= dto.StartTime)
                return BadRequest(new { message = "EndTime must be after StartTime." });

            try
            {
                var entry = await _context.TimetableEntries.FindAsync(id);
                if (entry == null)
                    return NotFound(new { message = "Timetable entry not found." });

                var conflict = await _context.TimetableEntries
                    .Where(te => te.Id != id && te.ClassId == dto.ClassId && te.Day == dto.Day)
                    .AnyAsync(te => dto.StartTime < te.EndTime && dto.EndTime > te.StartTime);
                if (conflict)
                    return BadRequest(new { message = "Timetable conflict detected for this class and day." });

                _mapper.Map(dto, entry);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Updated", "TimetableEntry", id.ToString());

                var response = await _context.TimetableEntries
                    .AsNoTracking()
                    .Include(te => te.Class)
                    .Include(te => te.Subject)
                    .FirstOrDefaultAsync(te => te.Id == id);

                return Ok(_mapper.Map<TimetableResponseDto>(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating timetable entry {Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTimetable(int id)
        {
            try
            {
                var entry = await _context.TimetableEntries.FindAsync(id);
                if (entry == null)
                    return NotFound(new { message = "Timetable entry not found." });

                _context.TimetableEntries.Remove(entry);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Deleted", "TimetableEntry", id.ToString());

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timetable entry {Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
