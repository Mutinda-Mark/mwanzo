using System.ComponentModel.DataAnnotations;

namespace mwanzo.DTOs
{
    public class SubjectCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class SubjectUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class SubjectResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
