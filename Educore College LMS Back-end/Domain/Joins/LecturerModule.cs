
using Educore_College_LMS_Back_end.Domain.Lecturers;
using Educore_College_LMS_Back_end.Domain.Modules;
namespace Educore_College_LMS_Back_end.Domain.Joins
{
    public class LecturerModule
    {
       
        
            public Guid LecturerId { get; set; }
            public Lecturer Lecturer { get; set; } = default!;

            public Guid ModuleId { get; set; }
            public Module Module { get; set; } = default!;
        
    }
}
