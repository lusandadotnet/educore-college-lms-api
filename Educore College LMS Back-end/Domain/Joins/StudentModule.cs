using Educore_College_LMS_Back_end.Domain.Students;
using Educore_College_LMS_Back_end.Domain.Modules;

namespace Educore_College_LMS_Back_end.Domain.Joins
{
    public class StudentModule
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = default!;

        public Guid ModuleId { get; set; }
        public Module Module
        {
            get; set;
        } = default!;
    }
}
