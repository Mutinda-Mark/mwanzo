using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Mathematics"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}