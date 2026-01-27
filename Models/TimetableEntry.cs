using System.ComponentModel.DataAnnotations;

namespace mwanzo.Models
{
    public class TimetableEntry
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}