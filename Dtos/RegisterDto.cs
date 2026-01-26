using System.ComponentModel.DataAnnotations;

namespace mwanzo.Dtos
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string Password { get; set; }

        // Optional: add more fields here, like Name, PhoneNumber, etc.
        // public string Name { get; set; }
    }
}
