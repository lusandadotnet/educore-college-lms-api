using Educore_College_LMS_Back_end.Data;
using Educore_College_LMS_Back_end.Domain.Enums;
using Educore_College_LMS_Back_end.Domain.Joins;
using Educore_College_LMS_Back_end.Domain.Tasks;
using Educore_College_LMS_Back_end.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore_College_LMS_Back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")] // Only students can access
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetStudentInfo(Guid studentId)
        {
            var student = await _context.Students
                .Where(s => s.Id == studentId)
                .Select(s => new
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    StudentNumber = s.StudentNumber
                    // Add other fields if needed
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound("Student not found.");

            return Ok(student);
        }
        [HttpGet("{studentId}/courses")]
        public async Task<IActionResult> GetEnrolledCourses(Guid studentId)
        {
            var courses = await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Include(sc => sc.Course)
                .Select(sc => new
                {
                    id = sc.Course.Id,
                    courseCode = sc.Course.CourseCode,
                    courseName = sc.Course.CourseName,
                    description = sc.Course.Description
                })
                .ToListAsync();

            return Ok(courses);
        }


        // GET: api/student/{studentId}/modules
        [HttpGet("{studentId}/modules")]
        public async Task<IActionResult> GetStudentModules(Guid studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return NotFound("Student not found.");

            var modules = await _context.StudentModules
                .Where(sm => sm.StudentId == studentId)
                .Select(sm => new
                {
                    sm.Module.Id,
                    sm.Module.ModuleName,
                    sm.Module.ModuleCode,
                    sm.Module.Description,
                    sm.Module.CourseId
                })
                .ToListAsync();

            return Ok(modules);
        }


        // -------------------------
        // 1. VIEW TASKS FOR ENROLLED MODULES
        // -------------------------
        [HttpGet("{studentId}/tasks")]
        public async Task<IActionResult> GetTasksForStudent(Guid studentId, [FromQuery] Guid? moduleId)
        {
            var query = _context.StudentTasks
                .Where(st => st.StudentId == studentId)
                .Include(st => st.TaskItem)
                .ThenInclude(t => t.Module)
                .AsQueryable();

            if (moduleId.HasValue)
                query = query.Where(st => st.TaskItem.ModuleId == moduleId.Value);

            var tasks = await query
                .Select(st => new
                {
                    TaskId = st.TaskItemId,
                    TaskName = st.TaskItem.TaskName,
                    DueDate = st.TaskItem.DueDate,
                    Status = st.Status.ToString()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetCourse(Guid courseId)
        {
            var course = await _context.Courses
                .Where(c => c.Id == courseId)
                .Select(c => new
                {
                    id = c.Id,
                    courseCode = c.CourseCode,
                    courseName = c.CourseName,
                    description = c.Description
                })
                .FirstOrDefaultAsync();

            if (course == null) return NotFound();

            return Ok(course);
        }
        // -------------------------
        // 2. UPDATE TASK STATUS
        // -------------------------
        // PUT: api/student/{studentId}/tasks/{taskId}/status
        [HttpPut("{studentId}/tasks/{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid studentId, Guid taskId, [FromBody] UpdateTaskStatusDto dto)
        {
            if (!Enum.TryParse<Domain.Enums.TaskStatus>(dto.Status, true, out var statusEnum))
                return BadRequest("Invalid status value");

            var studentTask = await _context.StudentTasks
                .FirstOrDefaultAsync(st => st.StudentId == studentId && st.TaskItemId == taskId);

            if (studentTask == null)
                return NotFound("Task not assigned to this student.");

            studentTask.Status = statusEnum;
            await _context.SaveChangesAsync();

            return Ok(new { TaskId = taskId, Status = studentTask.Status.ToString() });
        }




        // -------------------------
        // 3. FILTER TASKS BY STATUS
        // -------------------------
        [HttpGet("{studentId}/tasks/filter")]
        public async Task<IActionResult> FilterTasks(Guid studentId, [FromQuery] Domain.Enums.TaskStatus status)
        {
            var tasks = await _context.StudentTasks
                .Where(st => st.StudentId == studentId && st.Status == status)
                .Include(st => st.TaskItem)
                .Select(st => new
                {
                    TaskId = st.TaskItemId,
                    st.TaskItem.TaskName,
                    st.TaskItem.DueDate,
                    Status = st.Status.ToString()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        public class UpdateTaskStatusDto
        {
            public string Status { get; set; }
        }
    }
    

}
