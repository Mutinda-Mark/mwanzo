using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Grade 10A"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
    }
}