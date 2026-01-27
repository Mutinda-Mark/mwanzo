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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;

        public AttendanceController(ApplicationDbContext context, AuditService auditService, IMapper mapper)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpPost("mark")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> MarkAttendance([FromBody] AttendanceCreateDto dto)
        {
            var exists = await _context.Attendances
                .AnyAsync(a => a.StudentId == dto.StudentId && a.Date.Date == dto.Date.Date);

            if (exists) return BadRequest("Attendance already marked");

            var attendance = _mapper.Map<Attendance>(dto);
            attendance.IsLocked = true;
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Marked", "Attendance", attendance.Id.ToString());

            return Ok(_mapper.Map<AttendanceResponseDto>(attendance));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendance(int studentId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Attendances.Include(a => a.Student).ThenInclude(s => s.User).Where(a => a.StudentId == studentId);
            if (startDate.HasValue) query = query.Where(a => a.Date >= startDate);
            if (endDate.HasValue) query = query.Where(a => a.Date <= endDate);

            var attendances = await query.ToListAsync();
            return Ok(_mapper.Map<List<AttendanceResponseDto>>(attendances));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] AttendanceUpdateDto dto)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null || attendance.IsLocked) return BadRequest("Attendance not found or locked");

            _mapper.Map(dto, attendance);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Attendance", id.ToString());

            return Ok(_mapper.Map<AttendanceResponseDto>(attendance));
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
