using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class TimetableCreateDto
    {
        [Required]
        public int ClassId { get; set; }
        [Required]
        public int SubjectId { get; set; }
        [Required]
        public DayOfWeek Day { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
    }

    public class TimetableResponseDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
