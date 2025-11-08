using Educore_College_LMS_Back_end.Domain.Joins;
using Educore_College_LMS_Back_end.Domain.Modules;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Educore_College_LMS_Back_end.Domain.Tasks
{
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string TaskName { get; set; } = default!;

        public string Status { get; set; } = "NotStarted";

        [MaxLength(50)]
        public string? Description { get; set; } // optional now

        public DateOnly? DueDate { get; set; }

        public int? TotalMarks { get; set; } = 0; // optional for quizzes

        public Guid ModuleId { get; set; } // foreign key

        [JsonIgnore]
        public Module? Module { get; set; } // nullable now

        public ICollection<StudentTask> StudentTasks { get; set; } = new List<StudentTask>();
    }
}
