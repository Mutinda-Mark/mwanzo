using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using mwanzo.DTOs;
using mwanzo.Models;

namespace mwanzo.Controllers
{
    /// <summary>
    /// Handles user authentication and account operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user. Roles are system-controlled to prevent privilege escalation.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data.", errors = ModelState });

            try
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    AdmissionNumber = dto.AdmissionNumber
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("User registration failed for {Email}", dto.Email);
                    return BadRequest(new { message = "Unable to complete registration.", errors = result.Errors });
                }

                if (dto.Role == null)
                    return BadRequest(new { message = "Role is required." });

                var requestedRole = dto.Role.Value.ToString();

                // SECURITY: don't allow Admin from public register (recommended)
                var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Student",
                    "Teacher"
                    // Add "Admin" only if you truly want public admin creation (not recommended)
                };

                var roleToAssign = allowedRoles.Contains(requestedRole) ? requestedRole : "Student";

                var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return StatusCode(500, new { message = "User created but role assignment failed.", errors = roleResult.Errors });
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Auth",
                    new { userId = user.Id, token },
                    Request.Scheme
                );

                return Ok(new
                {
                    message = "User registered. Please confirm email.",
                    confrimLink = confirmLink, // keeping your existing FE key
                    userId = user.Id,
                    roleAssigned = roleToAssign
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Confirms a user's email.
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { message = "Invalid request." });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new { message = "Invalid request." });

            return Ok(new { message = "Email confirmed successfully." });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data.", errors = ModelState });

            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null || !user.EmailConfirmed ||
                    !await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                    return Unauthorized(new { message = "Invalid login credentials." });
                }

                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                var keyString = _config["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(keyString))
                    return StatusCode(500, new { message = "JWT Key not configured." });

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
