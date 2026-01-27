namespace mwanzo.Models
{
    public class SubjectAssignment
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;
    }
}