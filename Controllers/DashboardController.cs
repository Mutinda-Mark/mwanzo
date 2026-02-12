using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AutoMapper;
using mwanzo.Data;
using mwanzo.DTOs;
using Microsoft.AspNetCore.Identity;
using mwanzo.Models;

namespace mwanzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<DashboardController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

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

        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDashboard()
        {
            try
            {
                // ✅ Best way to get user id from the auth system
                var userId = _userManager.GetUserId(User);
                var email = User.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { message = "Missing user id claim." });

                var userIdTrim = userId.Trim();

                // 1) Normal case: Teachers.UserId == AspNetUsers.Id
                var teacher = await _context.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserId == userIdTrim);

                // 2) If stored with whitespace
                if (teacher == null)
                {
                    teacher = await _context.Teachers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.UserId != null && t.UserId.Trim() == userIdTrim);
                }

                // 3) If someone accidentally stored email in Teachers.UserId
                if (teacher == null && !string.IsNullOrWhiteSpace(email))
                {
                    var emailTrim = email.Trim();

                    teacher = await _context.Teachers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.UserId == emailTrim);

                    if (teacher == null)
                    {
                        teacher = await _context.Teachers
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.UserId != null && t.UserId.Trim() == emailTrim);
                    }
                }

                // 4) Final fallback: join to AspNetUsers (works even if Teachers.UserId is wrong)
                if (teacher == null && !string.IsNullOrWhiteSpace(email))
                {
                    var emailTrim = email.Trim();

                    teacher = await _context.Teachers
                        .AsNoTracking()
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.User != null && t.User.Email == emailTrim);
                }

                if (teacher == null)
                {
                    _logger.LogWarning("Teacher NOT found. userId={UserId}, email={Email}", userIdTrim, email);
                    return NotFound(new { message = "Teacher profile not found for this account." });
                }

                var classIds = await _context.SubjectAssignments
                    .AsNoTracking()
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Select(sa => sa.ClassId)
                    .Distinct()
                    .ToListAsync();

                var totalStudents = await _context.Students
                    .AsNoTracking()
                    .CountAsync(s => s.ClassId != null && classIds.Contains(s.ClassId.Value));

                var totalExams = await _context.Exams
                    .AsNoTracking()
                    .CountAsync(e => classIds.Contains(e.ClassId));

                return Ok(new TeacherDashboardDto
                {
                    TotalClasses = classIds.Count,
                    TotalStudents = totalStudents,
                    TotalExams = totalExams
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher dashboard");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            try
            {
                // ✅ Best way to get user id from Identity/JWT
                var userId = _userManager.GetUserId(User);
                var email = User.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { message = "Missing user id claim." });

                var userIdTrim = userId.Trim();

                // 1) Normal case: Students.UserId == AspNetUsers.Id
                var student = await _context.Students
                    .AsNoTracking()
                    .Include(s => s.Class)
                    .Include(s => s.Grades).ThenInclude(g => g.Exam)
                    .FirstOrDefaultAsync(s => s.UserId == userIdTrim);

                // 2) If stored with whitespace
                if (student == null)
                {
                    student = await _context.Students
                        .AsNoTracking()
                        .Include(s => s.Class)
                        .Include(s => s.Grades).ThenInclude(g => g.Exam)
                        .FirstOrDefaultAsync(s => s.UserId != null && s.UserId.Trim() == userIdTrim);
                }

                // 3) If someone accidentally stored email in Students.UserId
                if (student == null && !string.IsNullOrWhiteSpace(email))
                {
                    var emailTrim = email.Trim();

                    student = await _context.Students
                        .AsNoTracking()
                        .Include(s => s.Class)
                        .Include(s => s.Grades).ThenInclude(g => g.Exam)
                        .FirstOrDefaultAsync(s => s.UserId == emailTrim);

                    if (student == null)
                    {
                        student = await _context.Students
                            .AsNoTracking()
                            .Include(s => s.Class)
                            .Include(s => s.Grades).ThenInclude(g => g.Exam)
                            .FirstOrDefaultAsync(s => s.UserId != null && s.UserId.Trim() == emailTrim);
                    }
                }

                // 4) Final fallback: join to AspNetUsers via navigation (works if Students.UserId was wrong)
                if (student == null && !string.IsNullOrWhiteSpace(email))
                {
                    var emailTrim = email.Trim();

                    student = await _context.Students
                        .AsNoTracking()
                        .Include(s => s.User)
                        .Include(s => s.Class)
                        .Include(s => s.Grades).ThenInclude(g => g.Exam)
                        .FirstOrDefaultAsync(s => s.User != null && s.User.Email == emailTrim);
                }

                if (student == null)
                {
                    _logger.LogWarning("Student NOT found. userId={UserId}, email={Email}", userIdTrim, email);
                    return NotFound(new { message = "Student profile not found for this account." });
                }

                var average = student.Grades.Any()
                    ? (double)student.Grades.Average(g => g.Marks)
                    : 0;

                return Ok(new StudentDashboardDto
                {
                    ClassName = student.Class?.Name ?? "",
                    TotalExams = student.Grades.Count,
                    AverageGrade = average
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student dashboard");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }
}
