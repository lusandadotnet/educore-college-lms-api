using Educore_College_LMS_Back_end.Domain.Enums;
using Educore_College_LMS_Back_end.Domain.Students;
using Educore_College_LMS_Back_end.Domain.Tasks;
using TaskStatus = Educore_College_LMS_Back_end.Domain.Enums.TaskStatus;

namespace Educore_College_LMS_Back_end.Domain.Joins
{
    public class StudentTask
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    }
}
