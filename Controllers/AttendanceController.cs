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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public AttendanceController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost("mark")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> MarkAttendance([FromBody] Attendance attendance)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (attendance.Date.Date < DateTime.Today) return BadRequest("Cannot edit past attendance");

            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == attendance.StudentId && a.Date == attendance.Date);
            if (existing != null) return BadRequest("Attendance already marked");

            attendance.IsLocked = true; // Lock immediately
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Marked", "Attendance", attendance.Id.ToString());
            return Ok(attendance);
        }

        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetAttendance(int studentId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Attendances.Where(a => a.StudentId == studentId);
            if (startDate.HasValue) query = query.Where(a => a.Date >= startDate);
            if (endDate.HasValue) query = query.Where(a => a.Date <= endDate);
            var attendances = await query.ToListAsync();
            return Ok(attendances);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] Attendance updatedAttendance)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null || attendance.IsLocked) return BadRequest("Attendance not found or locked");

            attendance.IsPresent = updatedAttendance.IsPresent;
            attendance.Notes = updatedAttendance.Notes;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Attendance", id.ToString());
            return Ok(attendance);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Attendance", id.ToString());
            return NoContent();
        }

    }
}