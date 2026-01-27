using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mwanzo.Models;

namespace mwanzo.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<SubjectAssignment> SubjectAssignments { get; set; }
        public DbSet<TimetableEntry> TimetableEntries { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany() // No inverse navigation in ApplicationUser for simplicity
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithOne(u => u.Teacher) // One-to-one
                .HasForeignKey<Teacher>(t => t.UserId);

            // Add unique constraints (e.g., one teacher per subject per class)
            modelBuilder.Entity<SubjectAssignment>()
                .HasIndex(sa => new { sa.SubjectId, sa.ClassId })
                .IsUnique();

            // Timetable: No conflicts (you can add custom validation in controllers)
        }
    }
}