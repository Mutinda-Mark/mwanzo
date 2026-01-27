using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class Exam
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;

        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;

        public DateTime ExamDate { get; set; }
    }
}