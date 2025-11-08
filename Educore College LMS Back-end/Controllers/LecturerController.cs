using Educore_College_LMS_Back_end.Data;
using Educore_College_LMS_Back_end.Domain.Joins;
using Educore_College_LMS_Back_end.Domain.Enums;
using Educore_College_LMS_Back_end.Domain.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore_College_LMS_Back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- Get Lecturer Info ----------------
        [HttpGet("lecturers/{lecturerId}")]
        public async Task<IActionResult> GetLecturerInfo(Guid lecturerId)
        {
            var lecturer = await _context.Lecturers
                .Where(s => s.Id == lecturerId)
                .Select(s => new
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    LecturerNumber = s.LecturerNumber
                })
                .FirstOrDefaultAsync();

            if (lecturer == null)
                return NotFound("Lecturer not found.");

            return Ok(lecturer);
        }

        // ---------------- Get Courses with Assigned Modules ----------------
        public record CourseDto(Guid Id, string CourseName, IEnumerable<ModuleDto> Modules);
        public record ModuleDto(Guid Id, string ModuleName);

        [HttpGet("courses/{lecturerId}")]
        public async Task<IActionResult> GetLecturerCourses(Guid lecturerId)
        {
            var courses = await _context.Courses
                .Where(c => c.LecturerId == lecturerId)
                .Select(c => new CourseDto(
                    c.Id,
                    c.CourseName,
                    c.Modules.Select(m => new ModuleDto(m.Id, m.ModuleName))
                ))
                .ToListAsync();

            return Ok(courses);
        }

        // ---------------- Get Assigned Modules by Course ----------------
        [HttpGet("courses/{lecturerId}/{courseId}/modules")]
        public async Task<IActionResult> GetAssignedModulesByCourse(Guid lecturerId, Guid courseId)
        {
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (lecturer == null) return NotFound("Lecturer not found.");

            var modules = await _context.LecturerModules
                .Where(lm => lm.LecturerId == lecturerId && lm.Module.CourseId == courseId)
                .Select(lm => new
                {
                    Id = lm.Module.Id,
                    ModuleName = lm.Module.ModuleName,
                    ModuleCode = lm.Module.ModuleCode,
                    Description = lm.Module.Description,
                    CourseId = lm.Module.CourseId,
                    CourseName = lm.Module.Course.CourseName
                })
                .ToListAsync();

            return Ok(modules);
        }

        // ---------------- Create Task ----------------
        [HttpPost("modules/{moduleId}/tasks")]
        public async Task<IActionResult> CreateTask(Guid moduleId, [FromBody] TaskItem task)
        {
            var module = await _context.Modules
                .Include(m => m.StudentModules)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null) return NotFound("Module not found.");

            task.ModuleId = moduleId;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Assign task to all students in this module
            var enrolledStudents = module.StudentModules?.Select(sm => sm.StudentId).ToList() ?? new List<Guid>();
            foreach (var studentId in enrolledStudents)
            {
                _context.StudentTasks.Add(new StudentTask
                {
                    StudentId = studentId,
                    TaskItemId = task.Id,
                    Status = Domain.Enums.TaskStatus.NotStarted
                });
            }
            await _context.SaveChangesAsync();

            var response = new
            {
                task.TaskName,
                task.Description,
                task.TotalMarks,
                task.DueDate,
                task.Status,
                ModuleName = module.ModuleName
            };

            return CreatedAtAction(nameof(GetTasksByModule), new { moduleId }, response);
        }

        // ---------------- Get Tasks by Module ----------------
        [HttpGet("modules/{moduleId}/tasks")]
        public async Task<IActionResult> GetTasksByModule(Guid moduleId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ModuleId == moduleId)
                .Select(t => new
                {
                    t.Id,
                    t.TaskName,
                    t.Description,
                    t.TotalMarks,
                    t.DueDate,
                    t.Status
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // ---------------- Update Task ----------------
        [HttpPut("tasks/{taskId}")]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] TaskItem updatedTask)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound("Task not found.");

            task.TaskName = updatedTask.TaskName;
            task.Description = updatedTask.Description;
            task.TotalMarks = updatedTask.TotalMarks;
            task.DueDate = updatedTask.DueDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------------- Delete Task ----------------
        [HttpDelete("tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound("Task not found.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
