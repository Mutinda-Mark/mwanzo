using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // FK to ApplicationUser
        public virtual ApplicationUser User { get; set; } = null!;

        // Navigation
        public virtual ICollection<SubjectAssignment> SubjectAssignments { get; set; } = new List<SubjectAssignment>();
    }
}