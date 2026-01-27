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
    public class TimetableController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public TimetableController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTimetableEntry([FromBody] TimetableEntry entry)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Business rule: No conflicts (same class, day, overlapping time)
            var conflict = await _context.TimetableEntries
                .AnyAsync(te => te.ClassId == entry.ClassId && te.Day == entry.Day &&
                                ((entry.StartTime < te.EndTime && entry.EndTime > te.StartTime)));
            if (conflict) return BadRequest("Timetable conflict detected");

            _context.TimetableEntries.Add(entry);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "TimetableEntry", entry.Id.ToString());
            return Ok(entry);
        }

        [HttpGet("{classId}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetTimetable(int classId)
        {
            var timetable = await _context.TimetableEntries
                .Where(te => te.ClassId == classId)
                .Include(te => te.Subject)
                .ToListAsync();
            return Ok(timetable);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTimetableEntry(int id, [FromBody] TimetableEntry updatedEntry)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entry = await _context.TimetableEntries.FindAsync(id);
            if (entry == null) return NotFound();

            // Check for conflicts with the updated times
            var conflict = await _context.TimetableEntries
                .Where(te => te.Id != id) // Exclude current entry
                .AnyAsync(te => te.ClassId == updatedEntry.ClassId && te.Day == updatedEntry.Day &&
                                ((updatedEntry.StartTime < te.EndTime && updatedEntry.EndTime > te.StartTime)));
            if (conflict) return BadRequest("Timetable conflict detected with updated times");

            entry.Day = updatedEntry.Day;
            entry.StartTime = updatedEntry.StartTime;
            entry.EndTime = updatedEntry.EndTime;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "TimetableEntry", id.ToString(), $"TimetableEntry {id} updated");
            return Ok(entry);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTimetableEntry(int id)
        {
            var entry = await _context.TimetableEntries.FindAsync(id);
            if (entry == null) return NotFound();

            _context.TimetableEntries.Remove(entry);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "TimetableEntry", id.ToString(), $"TimetableEntry {id} deleted");
            return NoContent();
        }
    }
}