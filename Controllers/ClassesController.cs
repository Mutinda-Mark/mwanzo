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
    public class ClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ClassesController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] Class @class)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Classes.Add(@class);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Created", "Class", @class.Id.ToString());
            return CreatedAtAction(nameof(GetClass), new { id = @class.Id }, @class);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.TimetableEntries)
                .ToListAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetClass(int id)
        {
            var @class = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.TimetableEntries)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@class == null) return NotFound();
            return Ok(@class);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] Class updatedClass)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            @class.Name = updatedClass.Name;
            @class.Description = updatedClass.Description;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Updated", "Class", id.ToString(), $"Class {id} updated");
            return Ok(@class);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            _context.Classes.Remove(@class);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deleted", "Class", id.ToString(), $"Class {id} deleted");
            return NoContent();
        }
    }
}