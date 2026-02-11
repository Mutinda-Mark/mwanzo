export const endpoints = {
  auth: {
    register: "/api/Auth/register",
    login: "/api/Auth/login",
    confirmEmail: "/api/Auth/confirm-email",
  },

  dashboard: {
    admin: "/api/Dashboard/admin",
    teacher: "/api/Dashboard/teacher",
    student: "/api/Dashboard/student",
  },

  // CRUD resources
  subjects: "/api/Subjects",
  teachers: "/api/Teachers",
  teacherAssignSubject: "/api/Teachers/assign-subject",

  classes: "/api/Classes",
  students: "/api/Students",

  exams: "/api/Exams",
  grades: "/api/Grades",

  timetable: "/api/Timetable",

  // Attendance special routes
  attendance: "/api/Attendance",
  attendanceMark: "/api/Attendance/mark",
  attendanceByStudent: (studentId) => `/api/Attendance/student/${studentId}`,
};
