using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using mwanzo.Data;
using mwanzo.DTOs;

namespace mwanzo.Controllers
{
    /// <summary>
    /// Provides dashboard statistics for Admin, Teacher, and Student.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, IMapper mapper, ILogger<DashboardController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Admin dashboard metrics
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var dto = new AdminDashboardDto
                {
                    TotalStudents = await _context.Students.AsNoTracking().CountAsync(),
                    TotalTeachers = await _context.Teachers.AsNoTracking().CountAsync(),
                    TotalClasses = await _context.Classes.AsNoTracking().CountAsync(),
                    TotalExams = await _context.Exams.AsNoTracking().CountAsync()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching admin dashboard");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Teacher dashboard metrics
        /// </summary>
        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDashboard()
        {
            try
            {
                var teacherClaim = User.FindFirst("TeacherId")?.Value;
                if (string.IsNullOrEmpty(teacherClaim))
                    return Unauthorized(new { message = "Access denied." });

                if (!int.TryParse(teacherClaim, out var teacherId))
                    return BadRequest(new { message = "Invalid Teacher ID claim." });

                // Load classes taught by teacher with students count
                var classes = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacherId)
                    .Select(sa => sa.Class)
                    .Include(c => c.Students)
                    .AsNoTracking()
                    .Distinct()
                    .ToListAsync();

                var totalStudents = classes.Sum(c => c.Students.Count);

                // Optimized total exams
                var classIds = classes.Select(c => c.Id).ToList();
                var totalExams = await _context.Exams
                    .AsNoTracking()
                    .CountAsync(e => classIds.Contains(e.ClassId));

                var dto = new TeacherDashboardDto
                {
                    TotalClasses = classes.Count,
                    TotalStudents = totalStudents,
                    TotalExams = totalExams
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher dashboard");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Student dashboard metrics
        /// </summary>
        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            try
            {
                var studentClaim = User.FindFirst("StudentId")?.Value;
                if (string.IsNullOrEmpty(studentClaim))
                    return Unauthorized(new { message = "Access denied." });

                if (!int.TryParse(studentClaim, out var studentId))
                    return BadRequest(new { message = "Invalid Student ID claim." });

                var student = await _context.Students
                    .AsNoTracking()
                    .Include(s => s.Class)
                    .Include(s => s.Grades)
                        .ThenInclude(g => g.Exam)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                    return NotFound(new { message = "Student not found." });

                var average = student.Grades.Any() 
                    ? (double)student.Grades.Average(g => g.Marks) 
                    : 0;


                var dto = new StudentDashboardDto
                {
                    ClassName = student.Class?.Name ?? "",
                    TotalExams = student.Grades.Count,
                    AverageGrade = average
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student dashboard");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
