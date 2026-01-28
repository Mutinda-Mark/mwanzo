using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class StudentCreateDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }

    public class StudentUpdateDto
    {
        public int? ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }

    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
         public string Email { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? ClassName { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
