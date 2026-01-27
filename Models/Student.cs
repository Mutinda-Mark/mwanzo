using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // FK to ApplicationUser
        public virtual ApplicationUser User { get; set; } = null!;

        public int? ClassId { get; set; } // FK to Class (nullable for unenrolled)
        public virtual Class? Class { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        // Navigation for related data
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}