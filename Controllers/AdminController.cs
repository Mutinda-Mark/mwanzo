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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /api/Admin?q=search
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? q = null)
        {
            try
            {
                var query = _context.Users.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.Trim();

                    // Role is enum -> parse if possible
                    bool roleParsed = Enum.TryParse(typeof(UserRole), q, true, out var parsedRoleObj);
                    var parsedRole = roleParsed ? (UserRole)parsedRoleObj! : default;

                    query = query.Where(u =>
                        EF.Functions.Like(u.FirstName, $"%{q}%") ||
                        EF.Functions.Like(u.LastName, $"%{q}%") ||
                        (u.UserName != null && EF.Functions.Like(u.UserName, $"%{q}%")) ||
                        (u.AdmissionNumber != null && EF.Functions.Like(u.AdmissionNumber, $"%{q}%")) ||
                        (roleParsed && u.Role == parsedRole)
                    );
                }

                var users = await query
                    .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                    .Select(u => new AdminResponseDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        AdmissionNumber = u.AdmissionNumber ?? "",
                        UserName = u.UserName ?? "",
                        Role = u.Role.ToString()
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // GET: /api/Admin/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == id)
                    .Select(u => new AdminResponseDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        AdmissionNumber = u.AdmissionNumber ?? "",
                        UserName = u.UserName ?? "",
                        Role = u.Role.ToString()
                    })
                    .FirstOrDefaultAsync();

                if (user == null) return NotFound(new { message = "User not found." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // PUT: /api/Admin/{id}
        // Update safe fields only.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] AdminUpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null) return NotFound(new { message = "User not found." });

                // If username change requested, ensure unique
                if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
                {
                    var newUserName = dto.UserName.Trim();

                    bool userNameTaken = await _context.Users.AnyAsync(u => u.Id != id && u.UserName == newUserName);
                    if (userNameTaken) return BadRequest(new { message = "UserName is already taken." });

                    user.UserName = newUserName;
                    user.NormalizedUserName = newUserName.ToUpperInvariant();
                }

                user.FirstName = dto.FirstName.Trim();
                user.LastName = dto.LastName.Trim();
                user.AdmissionNumber = string.IsNullOrWhiteSpace(dto.AdmissionNumber) ? null : dto.AdmissionNumber.Trim();

                if (dto.Role.HasValue)
                {
                    user.Role = dto.Role.Value;
                }

                await _context.SaveChangesAsync();

                // return updated view dto
                var response = new AdminResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AdmissionNumber = user.AdmissionNumber ?? "",
                    UserName = user.UserName ?? "",
                    Role = user.Role.ToString()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // DELETE: /api/Admin/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null) return NotFound(new { message = "User not found." });

                // Clean related tables safely (if they exist)
                // Teacher is one-to-one
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == id);
                if (teacher != null)
                {
                    // Remove assignments first to avoid FK issues
                    var assignments = await _context.SubjectAssignments.Where(sa => sa.TeacherId == teacher.Id).ToListAsync();
                    if (assignments.Count > 0) _context.SubjectAssignments.RemoveRange(assignments);

                    _context.Teachers.Remove(teacher);
                }

                // Student rows reference UserId typically; adjust if your Student model uses different FK.
                var students = await _context.Students.Where(s => s.UserId == id).ToListAsync();
                if (students.Count > 0)
                {
                    _context.Students.RemoveRange(students);
                }

                // Finally remove identity user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
