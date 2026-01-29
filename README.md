# 🎓 Mwanzo School Management API

Mwanzo is a backend REST API built with **ASP.NET Core** for managing school operations including students, teachers, subjects, exams, grades, attendance, timetables, and dashboards.

It is designed with **clean architecture, role-based authorization**, and scalable database structure.

---

## 🚀 Features

- 🔐 JWT Authentication (Admin, Teacher, Student roles)
- 👨‍🏫 Teacher & Subject assignment system
- 🧑‍🎓 Student enrollment management
- 📝 Exams and grading
- 📊 Student performance reports
- 📅 Timetable scheduling with conflict detection
- ✅ Attendance tracking
- 📈 Dashboards per role

---

## 🛠 Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- AutoMapper
- JWT Authentication
- Swagger (API documentation)

---

## 🔑 Roles

| Role     | Capabilities |
|---------|--------------|
| Admin   | Full system control |
| Teacher | Grades, attendance, exams |
| Student | View results & timetable |

---

## ⚙️ Setup Instructions

1. Clone the repository
```bash
git clone https://github.com/your-username/mwanzo.git 
```
2. Configure database in appsettings.json

3. Run migrations
``` bash
dotnet ef database update
```

4. Run the API
``` bash
dotnet run
```

5. Access Swagger / Postman
``` bash
http://localhost:5101/swagger
```

---

## 📦 API Modules

- Auth
- Students
- Teachers
- Subjects
- Classes
- Exams
- Grades
- Attendance
- Timetable
- Dashboard

---

## 🧠 Project Goal
To provide a scalable, secure, and modular backend for modern school management systems.
