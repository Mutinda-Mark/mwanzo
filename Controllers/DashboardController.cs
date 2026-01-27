using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mwanzo.Data;
using mwanzo.Models;
using System.Linq;
using System.Security.Claims;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
    
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminOverview()
        {
            var totalStudents = await _context.Students.CountAsync();
            var totalTeachers = await _context.Teachers.CountAsync();
            var totalClasses = await _context.Classes.CountAsync();
            var totalExams = await _context.Exams.CountAsync();

            return Ok(new
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                TotalExams = totalExams
            });
        }

        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherWorkload()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var teacher = await _context.Teachers
                .Include(t => t.SubjectAssignments)
                .ThenInclude(sa => sa.Class)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null) return NotFound();

            var assignedClasses = teacher.SubjectAssignments.Select(sa => sa.Class.Name).Distinct().ToList();
            var totalAssignments = teacher.SubjectAssignments.Count;

            return Ok(new
            {
                AssignedClasses = assignedClasses,
                TotalSubjectAssignments = totalAssignments
            });
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentPerformance()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var student = await _context.Students
                .Include(s => s.Grades)
                .ThenInclude(g => g.Exam)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return NotFound();

            var averageGrade = student.Grades.Any() ? student.Grades.Average(g => g.Marks) : 0;
            var totalExams = student.Grades.Count;
            var attendanceRate = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .Select(a => a.IsPresent ? 1 : 0)
                .AverageAsync() * 100;

            return Ok(new
            {
                AverageGrade = averageGrade,
                TotalExams = totalExams,
                AttendanceRate = attendanceRate
            });
        }
    }
}