using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class GradeCreateDto
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public int ExamId { get; set; }
        [Required, Range(0, 100)]
        public decimal Marks { get; set; }
        public string? Comments { get; set; }
    }

    public class GradeUpdateDto
    {
        [Range(0, 100)]
        public decimal Marks { get; set; }
        public string? Comments { get; set; }
    }

    public class GradeResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public decimal Marks { get; set; }
        public string? Comments { get; set; }
    }
}
