using System.ComponentModel.DataAnnotations;
using mwanzo.Models;

namespace mwanzo.DTOs
{
    public class RegisterRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public string? AdmissionNumber { get; set; }
    }

    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class AuthResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string ConfirmLink { get; set; } = string.Empty;
    }
}
