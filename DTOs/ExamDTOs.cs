using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class ExamCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int SubjectId { get; set; }
        [Required]
        public int ClassId { get; set; }
        [Required]
        public DateTime ExamDate { get; set; }
    }

    public class ExamResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
    }
}
