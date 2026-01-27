using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public int ExamId { get; set; }
        public virtual Exam Exam { get; set; } = null!;

        [Range(0, 100)]
        public decimal Marks { get; set; } // e.g., 85.5

        public string? Comments { get; set; }
    }
}