using Educore_College_LMS_Back_end.Controllers;
using Educore_College_LMS_Back_end.Domain.Courses;
using Educore_College_LMS_Back_end.Domain.Tasks;
using System.ComponentModel.DataAnnotations;
using Educore_College_LMS_Back_end.Domain.Joins;

namespace Educore_College_LMS_Back_end.Domain.Modules
{
    public class Module
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ICollection<LecturerModule> LecturerModules { get; set; }
        public ICollection<StudentModule> StudentModules { get; set; }
        

        [Required]
        [MaxLength(50)]
        public string ModuleName { get; set; } = default!;

        [Required]
        [MaxLength(10)]
        public string ModuleCode { get; set; } = default!;

        public string? Description { get; set; }

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = default!;
    

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    }
}
