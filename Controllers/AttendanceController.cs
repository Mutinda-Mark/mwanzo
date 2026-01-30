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
    /// <summary>
    /// Handles student attendance operations.
    /// All endpoints require authenticated users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            ApplicationDbContext context,
            AuditService auditService,
            IMapper mapper,
            ILogger<AttendanceController> logger)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Marks attendance for a student on a given date.
        /// Only Admins and Teachers can perform this action.
        /// </summary>
        [HttpPost("mark")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> MarkAttendance([FromBody] AttendanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data." });

            try
            {
                var exists = await _context.Attendances
                    .AnyAsync(a => a.StudentId == dto.StudentId && a.Date.Date == dto.Date.Date);

                if (exists)
                    return BadRequest(new { message = "Request could not be processed." });

                var attendance = _mapper.Map<Attendance>(dto);
                attendance.IsLocked = true;

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Marked", "Attendance", attendance.Id.ToString());

                var response = _mapper.Map<AttendanceResponseDto>(attendance);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves attendance records for a student within an optional date range.
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendance(int studentId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.Attendances
                    .AsNoTracking()
                    .Where(a => a.StudentId == studentId);

                if (startDate.HasValue)
                    query = query.Where(a => a.Date.Date >= startDate.Value.Date);

                if (endDate.HasValue)
                    query = query.Where(a => a.Date.Date <= endDate.Value.Date);

                var attendances = await query.ToListAsync();
                return Ok(_mapper.Map<List<AttendanceResponseDto>>(attendances));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Updates an attendance record.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] AttendanceUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data." });

            try
            {
                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null)
                    return NotFound(new { message = "Resource not found." });

                _mapper.Map(dto, attendance);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Updated", "Attendance", attendance.Id.ToString());

                return Ok(_mapper.Map<AttendanceResponseDto>(attendance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance {AttendanceId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Deletes an attendance record.
        /// Admin only.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null)
                    return NotFound(new { message = "Resource not found." });

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Deleted", "Attendance", id.ToString());

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance {AttendanceId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
