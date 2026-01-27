using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Add this using

namespace mwanzo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        // Unique identifier for students (e.g., "STU001"). Nullable for non-students.
        [StringLength(20)]
        public string? AdmissionNumber { get; set; }

        // Role as enum (maps to Identity roles, but stored here for quick access)
        public UserRole Role { get; set; }

        // Status (Active, Graduated, Suspended)
        public UserStatus Status { get; set; } = UserStatus.Active;

        // Relationships (navigation properties)
        public virtual ICollection<Student> Students { get; set; } = new List<Student>(); // For parents
        public virtual Teacher? Teacher { get; set; } // One-to-one with Teacher if role is Teacher
    }

    [JsonConverter(typeof(JsonStringEnumConverter))] // Add this line
    public enum UserRole
    {
        Admin,
        Teacher,
        Student,
        Parent
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserStatus
    {
        Active,
        Graduated,
        Suspended
    }
}