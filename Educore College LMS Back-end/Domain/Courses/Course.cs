using Educore_College_LMS_Back_end.Domain.Joins;
using Educore_College_LMS_Back_end.Domain.Lecturers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Educore_College_LMS_Back_end.Domain.Modules;

namespace Educore_College_LMS_Back_end.Domain.Courses
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CourseName { get; set; } = default!;

        [Required]
        [MaxLength(10)]
        public string CourseCode { get; set; } = default!;

        [MaxLength(200)]
        public string? Description { get; set; }

        public Guid? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }

        public ICollection<Domain.Modules.Module> Modules { get; set; } = new List<Domain.Modules.Module>();
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    }
}
