using AutoMapper;
using mwanzo.Models;
using mwanzo.DTOs;

namespace mwanzo.MappingProfiles
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            // Auth/User mapping
            CreateMap<ApplicationUser, StudentResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.AdmissionNumber, opt => opt.MapFrom(src => src.AdmissionNumber))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Students.FirstOrDefault().Class.Name));

            CreateMap<StudentCreateDto, Student>();
            CreateMap<StudentUpdateDto, Student>();

            CreateMap<TeacherCreateDto, Teacher>();
            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.SubjectAssignments, opt => opt.MapFrom(src => src.SubjectAssignments));

            CreateMap<SubjectAssignment, SubjectAssignmentResponseDto>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name));

            CreateMap<ClassCreateDto, Class>();
            CreateMap<ClassUpdateDto, Class>();
            CreateMap<Class, ClassResponseDto>()
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Students.Count));

            CreateMap<ExamCreateDto, Exam>();
            CreateMap<Exam, ExamResponseDto>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name));

            CreateMap<GradeCreateDto, Grade>();
            CreateMap<GradeUpdateDto, Grade>();
            CreateMap<Grade, GradeResponseDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FirstName + " " + src.Student.User.LastName))
                .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.Name));

            CreateMap<AttendanceCreateDto, Attendance>();
            CreateMap<AttendanceUpdateDto, Attendance>();
            CreateMap<Attendance, AttendanceResponseDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FirstName + " " + src.Student.User.LastName));

            CreateMap<TimetableCreateDto, TimetableEntry>();
            CreateMap<TimetableEntry, TimetableResponseDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name));
        }
    }
}
