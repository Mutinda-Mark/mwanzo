using System.ComponentModel.DataAnnotations;
using mwanzo.Models;

namespace mwanzo.DTOs
{
    public class AdminResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // nullable in DB -> safe output
        public string AdmissionNumber { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        // enum -> string
        public string Role { get; set; } = string.Empty;
    }

    public class AdminUpdateUserDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        // Optional (nullable)
        [StringLength(20)]
        public string? AdmissionNumber { get; set; }

        // Optional: if provided, update username
        [StringLength(256)]
        public string? UserName { get; set; }

        // If provided, update enum role
        public UserRole? Role { get; set; }
    }
}
