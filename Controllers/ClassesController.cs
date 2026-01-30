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
    /// Handles class management operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(ApplicationDbContext context, AuditService auditService, IMapper mapper, ILogger<ClassesController> logger)
        {
            _context = context;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new class. Admin only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var @class = _mapper.Map<Class>(dto);
                _context.Classes.Add(@class);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Created", "Class", @class.Id.ToString());

                return CreatedAtAction(nameof(GetClass), new { id = @class.Id }, _mapper.Map<ClassResponseDto>(@class));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves all classes. Admin & Teacher access.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetClasses()
        {
            try
            {
                var classes = await _context.Classes
                    .AsNoTracking()
                    .Include(c => c.Students)
                    .Include(c => c.TimetableEntries)
                    .ToListAsync();

                return Ok(_mapper.Map<List<ClassResponseDto>>(classes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving classes.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves a single class by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClass(int id)
        {
            try
            {
                var @class = await _context.Classes
                    .AsNoTracking()
                    .Include(c => c.Students)
                    .Include(c => c.TimetableEntries)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (@class == null)
                    return NotFound(new { message = "Resource not found." });

                return Ok(_mapper.Map<ClassResponseDto>(@class));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving class {ClassId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Updates an existing class. Admin only.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var @class = await _context.Classes.FindAsync(id);
                if (@class == null)
                    return NotFound(new { message = "Resource not found." });

                _mapper.Map(dto, @class);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Updated", "Class", id.ToString());

                return Ok(_mapper.Map<ClassResponseDto>(@class));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class {ClassId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Deletes a class. Admin only.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                var @class = await _context.Classes.FindAsync(id);
                if (@class == null)
                    return NotFound(new { message = "Resource not found." });

                _context.Classes.Remove(@class);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Deleted", "Class", id.ToString());

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class {ClassId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
