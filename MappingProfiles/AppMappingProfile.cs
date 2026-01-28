using AutoMapper;
using mwanzo.Models;
using mwanzo.DTOs;

namespace mwanzo.MappingProfiles
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            // Teacher mappings
            CreateMap<TeacherCreateDto, Teacher>();
            
            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.SubjectAssignments, opt => opt.MapFrom(src => src.SubjectAssignments));

            
            // SubjectAssignment mappings
            CreateMap<SubjectAssignmentCreateDto, SubjectAssignment>()
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore());

            CreateMap<SubjectAssignment, SubjectAssignmentResponseDto>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name));

            // Students (example)
            CreateMap<StudentCreateDto, Student>();
            CreateMap<StudentUpdateDto, Student>();
            CreateMap<Student, StudentResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name));

            // Classes
            CreateMap<ClassCreateDto, Class>();
            CreateMap<ClassUpdateDto, Class>();
            CreateMap<Class, ClassResponseDto>()
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Students.Count));

            // Exams
            CreateMap<ExamCreateDto, Exam>();
            CreateMap<Exam, ExamResponseDto>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name));

            // Grades
            CreateMap<GradeCreateDto, Grade>();
            CreateMap<GradeUpdateDto, Grade>();
            CreateMap<Grade, GradeResponseDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FirstName + " " + src.Student.User.LastName))
                .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.Name));

            // Attendance
            CreateMap<AttendanceCreateDto, Attendance>();
            CreateMap<AttendanceUpdateDto, Attendance>();
            CreateMap<Attendance, AttendanceResponseDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FirstName + " " + src.Student.User.LastName));

            // Timetable
            CreateMap<TimetableCreateDto, TimetableEntry>();
            CreateMap<TimetableEntry, TimetableResponseDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name));

            // Subject mappings
            CreateMap<SubjectCreateDto, Subject>();
            CreateMap<SubjectUpdateDto, Subject>();
            CreateMap<Subject, SubjectResponseDto>();

        }
    }
}
