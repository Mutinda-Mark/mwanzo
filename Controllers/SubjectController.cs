using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mwanzo.Data;
using mwanzo.DTOs;
using mwanzo.Models;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SubjectsController> _logger;

        public SubjectsController(ApplicationDbContext context, IMapper mapper, ILogger<SubjectsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Subjects
        [HttpGet]
        public async Task<IActionResult> GetSubjects()
        {
            try
            {
                var subjects = await _context.Subjects
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(_mapper.Map<List<SubjectResponseDto>>(subjects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subjects");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // GET: api/Subjects/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubject(int id)
        {
            try
            {
                var subject = await _context.Subjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subject == null)
                    return NotFound(new { message = "Subject not found" });

                return Ok(_mapper.Map<SubjectResponseDto>(subject));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject {SubjectId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // POST: api/Subjects
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                bool exists = await _context.Subjects
                    .AnyAsync(s => s.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                    return BadRequest(new { message = "Subject already exists." });

                var subject = _mapper.Map<Subject>(dto);
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<SubjectResponseDto>(subject);

                return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // PUT: api/Subjects/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] SubjectUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                    return NotFound(new { message = "Subject not found" });

                bool exists = await _context.Subjects
                    .AnyAsync(s => s.Id != id && s.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                    return BadRequest(new { message = "Another subject with this name already exists." });

                _mapper.Map(dto, subject);
                await _context.SaveChangesAsync();

                return Ok(_mapper.Map<SubjectResponseDto>(subject));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject {SubjectId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // DELETE: api/Subjects/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                    return NotFound(new { message = "Subject not found" });

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Foreign key conflict deleting subject {SubjectId}", id);
                return BadRequest(new { message = "Subject cannot be deleted because it is in use." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject {SubjectId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
