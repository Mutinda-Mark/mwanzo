using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class AttendanceCreateDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        public string? Notes { get; set; }
    }

    public class AttendanceUpdateDto
    {
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }

    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }
}
