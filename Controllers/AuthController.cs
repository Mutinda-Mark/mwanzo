using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
//using mwanzo.Dtos;
using System.Net.Mail;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

       
        [HttpPost("register")]
        public async Task<IActionResult> Register(string email, string password)
        {
            try
            {
                var mail = new MailAddress(email);
            }
            catch
            {
                return BadRequest(new { Message = "Invalid email format" });
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmLink = Url.Action(nameof(ConfirmEmail), "Auth",
                new { userId = user.Id, token }, Request.Scheme);

            return Ok(new
            {
                Message = "User registered. Please confirm email.",
                UserId = user.Id,
                Token = token,
                ConfirmLink = confirmLink
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
                return BadRequest(new { Message = "Invalid user" });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) 
                return BadRequest(new { Message = "Email confirmation failed" });

            return Ok(new 
            { 
                Message = "Email confirmed successfully",
                UserId = user.Id
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Check if user exists, email confirmed, and password is correct
            var loginSuccessful = user != null 
                                && user.EmailConfirmed 
                                && await _userManager.CheckPasswordAsync(user, password);

            if (!loginSuccessful)
            {
                // Generic error message for security
                return Unauthorized(new { Message = "Invalid login credentials" });
            }

            // Create JWT claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

    }
}
