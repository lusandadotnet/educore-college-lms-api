using Educore_College_LMS_Back_end.Domain.Courses;
using Educore_College_LMS_Back_end.Domain.Students;

namespace Educore_College_LMS_Back_end.Domain.Joins
{
    public class StudentCourse
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = default!;

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = default!;

        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
    }
}
