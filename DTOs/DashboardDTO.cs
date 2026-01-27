public class AdminDashboardDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalExams { get; set; }
}

public class TeacherDashboardDto
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalExams { get; set; }
}

public class StudentDashboardDto
{
    public string ClassName { get; set; }
    public int TotalExams { get; set; }
    public double AverageGrade { get; set; }
}
