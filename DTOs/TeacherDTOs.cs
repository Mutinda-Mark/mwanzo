using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class TeacherCreateDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
    }

    public class TeacherResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; }
        public List<SubjectAssignmentResponseDto> SubjectAssignments { get; set; } = new();
    }

    public class SubjectAssignmentCreateDto
{
    [Required]
    public string TeacherId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassId { get; set; }
}

public class SubjectAssignmentUpdateDto
{
    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassId { get; set; }
}

public class SubjectAssignmentResponseDto
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
}

}
