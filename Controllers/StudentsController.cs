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
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ApplicationDbContext context, AuditService auditService, IMapper mapper, ILogger<StudentsController> logger)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        // CREATE STUDENT
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
                if (!userExists)
                    return BadRequest(new { message = "Invalid user." });

                bool alreadyStudent = await _context.Students
                    .AnyAsync(s => s.UserId == dto.UserId);

                if (alreadyStudent)
                    return BadRequest(new { message = "This user is already registered as a student." });

                if (dto.ClassId.HasValue)
                {
                    bool classExists = await _context.Classes.AnyAsync(c => c.Id == dto.ClassId);
                    if (!classExists)
                        return BadRequest(new { message = "Invalid class." });
                }

                var student = _mapper.Map<Student>(dto);
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Created", "Student", student.Id.ToString());

                var response = await _context.Students
                    .AsNoTracking()
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Where(s => s.Id == student.Id)
                    .Select(s => _mapper.Map<StudentResponseDto>(s))
                    .FirstAsync();

                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // GET STUDENT
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            try
            {
                var student = await _context.Students
                    .AsNoTracking()
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                    return NotFound(new { message = "Student not found" });

                return Ok(_mapper.Map<StudentResponseDto>(student));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student {StudentId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // UPDATE STUDENT
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentUpdateDto dto)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound(new { message = "Student not found" });

                if (dto.ClassId.HasValue)
                {
                    bool classExists = await _context.Classes.AnyAsync(c => c.Id == dto.ClassId);
                    if (!classExists)
                        return BadRequest(new { message = "Invalid class." });
                }

                _mapper.Map(dto, student);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Updated", "Student", id.ToString());

                return Ok(_mapper.Map<StudentResponseDto>(student));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // DELETE STUDENT
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound(new { message = "Student not found" });

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Deleted", "Student", id.ToString());

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "FK constraint deleting student {StudentId}", id);
                return BadRequest(new { message = "Student cannot be deleted because it is referenced elsewhere." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
