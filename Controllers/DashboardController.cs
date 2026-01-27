using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using mwanzo.Data;
using mwanzo.DTOs;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var dto = new AdminDashboardDto
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalExams = await _context.Exams.CountAsync()
            };

            return Ok(dto);
        }

        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDashboard()
        {
            var teacherClaim = User.FindFirst("TeacherId")?.Value;
            if (teacherClaim == null) return Unauthorized();
            var teacherId = int.Parse(teacherClaim);

            var classes = await _context.SubjectAssignments
                .Where(sa => sa.TeacherId == teacherId)
                .Select(sa => sa.Class)
                .Distinct()
                .ToListAsync();

            var totalStudents = classes.Sum(c => c.Students.Count);
            var totalExams = await _context.Exams.CountAsync(e => classes.Select(c => c.Id).Contains(e.ClassId));

            var dto = new TeacherDashboardDto
            {
                TotalClasses = classes.Count,
                TotalStudents = totalStudents,
                TotalExams = totalExams
            };

            return Ok(dto);
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var studentClaim = User.FindFirst("StudentId")?.Value;
            if (studentClaim == null) return Unauthorized();
            var studentId = int.Parse(studentClaim);

            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Exam)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return NotFound();

            var average = student.Grades.Any() ? (double)student.Grades.Average(g => g.Marks) : 0;

            var dto = new StudentDashboardDto
            {
                ClassName = student.Class?.Name ?? "",
                TotalExams = student.Grades.Count,
                AverageGrade = average
            };

            return Ok(dto);
        }
    }
}
