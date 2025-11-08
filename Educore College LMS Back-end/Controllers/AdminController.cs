using Educore_College_LMS_Back_end.Data;
using Educore_College_LMS_Back_end.Domain.Students;
using Educore_College_LMS_Back_end.Domain.Lecturers;
using Educore_College_LMS_Back_end.Domain.Courses;
using Educore_College_LMS_Back_end.Domain.Modules;
using Educore_College_LMS_Back_end.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Educore_College_LMS_Back_end.Domain.Joins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Educore_College_LMS_Back_end.DTOs;
using Microsoft.AspNetCore.Identity;
using Educore_College_LMS_Back_end.Services;

namespace Educore_College_LMS_Back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only allow users with the "Admin" role to access this controller
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;


        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        private string GenerateRandomPassword(int length = 8)
        {
            if (length < 8)
                length = 8; // enforce minimum length

            var random = new Random();

            const string upper = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*_-?";

            // pick one of each required category
            var passwordChars = new List<char>
            {
                 upper[random.Next(upper.Length)],
                 lower[random.Next(lower.Length)],
                 digits[random.Next(digits.Length)],
                 special[random.Next(special.Length)]
            };

            // fill the rest with a mix of everything
            string allChars = upper + lower + digits + special;
            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }

            // shuffle to avoid predictable order
            return new string(passwordChars.OrderBy(_ => random.Next()).ToArray());
        }
        //-------------------
        // STUDENTS CRUD
        //-------------------

        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students.ToListAsync();
            return Ok(students);
        }

        [HttpPost("students")]
        public async Task<IActionResult> AddStudent([FromBody] StudentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Generate a strong temp password
            var tempPassword = GenerateRandomPassword();
            

            // 2. Create the Student entity first (but don’t save yet)
            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                HomeAddress = dto.HomeAddress,
                PhoneNumber = dto.PhoneNumber
            };

            // 3. Create the Identity user
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                MustChangePassword = true
            };

            var identityResult = await _userManager.CreateAsync(user, tempPassword);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            // 4. Add the user to the Student role
            await _userManager.AddToRoleAsync(user, "Student");

            // 5. Link the Identity user to the Student record
            student.UserId = user.Id;

            // 6. Generate unique student number
            string studentNumber;
            do
            {
                studentNumber = "s" + new Random().Next(10000000, 999999999).ToString();
            } while (await _context.Students.AnyAsync(s => s.StudentNumber == studentNumber));

            student.StudentNumber = studentNumber;

            // 7. Save student to the database
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // 8. Send welcome email
            await _emailSender.SendEmailAsync(
               dto.Email,
               "Welcome to Educore - Your Account Credentials",
                            $@"
                      <div style='font-family: Arial, sans-serif; max-width:600px; margin:auto; border:1px solid #e0e0e0; border-radius:8px; overflow:hidden;'>
                        <div style='background-color:#2c3e50; padding:20px; text-align:center;'>
                          <h2 style='color:white; margin:0;'>Educore College LMS</h2>
                        </div>
                        <div style='padding:20px;'>
                          <p>Hi <strong>{dto.FirstName}</strong>,</p>
                          <p>Your account has been created successfully. Please use the credentials below to log in:</p>

                          <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
                              <tr>
                                  <td style='padding:8px; border:1px solid #ddd; background:#f9f9f9; width:40%;'><strong>Email</strong></td>
                                  <td style='padding:8px; border:1px solid #ddd;'>{dto.Email}</td>
                              </tr>
                              <tr>
                                  <td style='padding:8px; border:1px solid #ddd; background:#f9f9f9;'><strong>Temporary Password</strong></td>
                                  <td style='padding:8px; border:1px solid #ddd; color:#e74c3c; font-weight:bold;'>{tempPassword}</td>
                              </tr>
                          </table>

                          <p style='color:#555;'>⚠️ You will be required to change your password the first time you log in.</p>
                          <p>Best regards,<br/>Educore Admin Team</p>
                        </div>
                      </div>
                    ");

            return CreatedAtAction(nameof(GetAllStudents), new { id = student.Id }, student);
        }

        [HttpPut("students/{id}")]
        public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentDto dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.Gender = dto.Gender ?? student.Gender;
            student.DateOfBirth = dto.DateOfBirth ?? student.DateOfBirth;
            student.HomeAddress = dto.HomeAddress ?? student.HomeAddress;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Email = dto.Email ?? student.Email;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //-------------------
        // LECTURERS CRUD
        //-------------------

        [HttpGet("lecturers")]
        public async Task<IActionResult> GetAllLecturers()
        {
            var lecturers = await _context.Lecturers.ToListAsync();
            return Ok(lecturers);
        }

        [HttpPost("lecturers")]
        public async Task<IActionResult> AddLecturer([FromBody] LecturerDto lecturerdto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tempPassword = GenerateRandomPassword();

            var lecturer = new Lecturer
            {
                FirstName = lecturerdto.FirstName,
                LastName = lecturerdto.LastName,
                Email = lecturerdto.Email,
                PhoneNumber = lecturerdto.PhoneNumber

            };

            var user = new ApplicationUser
            {
                UserName = lecturerdto.Email,
                Email = lecturerdto.Email,
                LecturerId = lecturer.Id,
                MustChangePassword = true
            };

            var identityResult = await _userManager.CreateAsync(user, tempPassword);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Lecturer");

            // Link the Identity user to the Lecturer record
            lecturer.UserId = user.Id;

            string lecturerNumber;
            do
            {
                lecturerNumber = new Random().Next(1000, 9999).ToString();
            } while (await _context.Lecturers.AnyAsync(l => l.LecturerNumber == lecturerNumber));

            lecturer.LecturerNumber = lecturerNumber;

            await _emailSender.SendEmailAsync(
                lecturerdto.Email,
                "Welcome to Educore - Your Account Credentials",
            $@"
                 <div style='font-family: Arial, sans-serif; max-width:600px; margin:auto; border:1px solid #e0e0e0; border-radius:8px; overflow:hidden;'>
                 <div style='background-color:#2c3e50; padding:20px; text-align:center;'>
                 <h2 style='color:white; margin:0;'>Educore College LMS</h2>
                 </div>
                 <div style='padding:20px;'>
                 <p>Hi <strong>{lecturerdto.FirstName}</strong>,</p>
                 <p>Your account has been created successfully. Please use the credentials below to log in:</p>

                 <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
                    <tr>
                        <td style='padding:8px; border:1px solid #ddd; background:#f9f9f9; width:40%;'><strong>Email</strong></td>
                        <td style='padding:8px; border:1px solid #ddd;'>{lecturerdto.Email}</td>
                    </tr>
                    <tr>
                        <td style='padding:8px; border:1px solid #ddd; background:#f9f9f9;'><strong>Temporary Password</strong></td>
                        <td style='padding:8px; border:1px solid #ddd; color:#e74c3c; font-weight:bold;'>{tempPassword}</td>
                    </tr>
                </table>

                <p style='color:#555;'>⚠️ For security reasons, you will be required to change your password the first time you log in.</p>

                <p>Click the button below to access the Educore LMS:</p>

                <div style='text-align:center; margin:20px 0;'>
                    <a href='https://your-frontend-login-url.com' 
                       style='display:inline-block; background-color:#3498db; color:white; padding:12px 24px; 
                              text-decoration:none; border-radius:5px; font-weight:bold;'>Login Now</a>
                </div>

                <p>If you did not request this account, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>Educore Admin Team</p>
                 </div>
                <div style='background-color:#f4f4f4; padding:10px; text-align:center; font-size:12px; color:#888;'>
                © {DateTime.Now.Year} Educore College. All rights reserved.
                 </div>
                </div>
            ");


            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllLecturers), new { id = lecturer.Id }, lecturer);
        }

        [HttpPut("lecturers/{id}")]
        public async Task<IActionResult> UpdateLecturer(Guid id, [FromBody] UpdateLecturerDto dto)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null) return NotFound();

            lecturer.FirstName = dto.FirstName ?? lecturer.FirstName;
            lecturer.LastName = dto.LastName ?? lecturer.LastName;
            lecturer.PhoneNumber = dto.PhoneNumber ?? lecturer.PhoneNumber;
            lecturer.Email = dto.Email ?? lecturer.Email;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("lecturers/{id}")]
        public async Task<IActionResult> DeleteLecturer(Guid id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
            {
                return NotFound();
            }
            _context.Lecturers.Remove(lecturer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //-------------------
        // COURSES CRUD
        //-------------------

        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
             .Include(c => c.Lecturer)
             .Select(c => new CourseDto
             {
                 Id = c.Id,
                 CourseCode = c.CourseCode,
                 CourseName = c.CourseName,
                 Description = c.Description,
                 Lecturer = c.Lecturer == null ? null : new LecturerDto
                 {
                     Id = c.Lecturer.Id,
                     FirstName = c.Lecturer.FirstName,
                     LastName = c.Lecturer.LastName
                 }
             })
             .ToListAsync();


            return Ok(courses);
        }

        [HttpPost("courses")]
        public async Task<IActionResult> AddCourse([FromBody] CourseDto coursedto)
        {
            var course = new Course
            {
                CourseName = coursedto.CourseName,
                CourseCode = coursedto.CourseCode,
                Description = coursedto.Description
                
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllCourses), new { id = course.Id }, course);
        }


        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.CourseName = dto.CourseName;
            course.CourseCode = dto.CourseCode;
            course.Description = dto.Description ?? course.Description;
            course.LecturerId = dto.LecturerId ?? course.LecturerId;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //-------------------
        // MODULES CRUD
        //-------------------

        [HttpGet("modules")]
        public async Task<IActionResult> GetAllModules()
        {
            var modules = await _context.Modules
             .Select(m => new ModuleDto
             {
                 Id = m.Id,
                 ModuleCode = m.ModuleCode,
                 ModuleName = m.ModuleName,
                 Description = m.Description,
                 CourseId = m.CourseId
             })
             .ToListAsync();

            return Ok(modules);
        }

        [HttpPost("modules")]
        public async Task<IActionResult> AddModule([FromBody] ModuleDto moduledto)
        {
            var module = new Module
            {

                ModuleName = moduledto.ModuleName,
                ModuleCode = moduledto.ModuleCode,
                Description = moduledto.Description,
                CourseId = moduledto.CourseId
            };
            _context.Modules.Add(module);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllModules), new { id = module.Id }, module);
        }

        [HttpPut("modules/{id}")]
        public async Task<IActionResult> UpdateModule(Guid id, [FromBody] UpdateModuleDto dto)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null) return NotFound();

            module.ModuleName = dto.ModuleName;
            module.ModuleCode = dto.ModuleCode;
            module.Description = dto.Description ?? module.Description;
            module.CourseId = dto.CourseId ?? module.CourseId;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("modules/{id}")]
        public async Task<IActionResult> DeleteModule(Guid id)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //---------------------------------
        //Assign Lecturer to Course
        //---------------------------------

        [HttpPost("courses/{courseId}/assign-lecturer/{lecturerId}")]
        public async Task<IActionResult> AssignLecturerToCourse(Guid courseId, Guid lecturerId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (course == null || lecturer == null)
            {
                return NotFound("Course or Lecturer not found.");
            }
            course.LecturerId = lecturerId;
            await _context.SaveChangesAsync();
            return Ok($"Lecturer {lecturer.FirstName} assigned to course {course.CourseName}.");
        }

        //--------------------------------
        //Assign Lecturer to modules
        //-------------------------------
        [HttpPost("modules/{moduleId}/assign-lecturer/{lecturerId}")]
        public async Task<IActionResult> AssignLecturerToModule(Guid moduleId, Guid lecturerId)
        {
            var module = await _context.Modules.FindAsync(moduleId);
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (module == null || lecturer == null)
            {
                return NotFound("Module or Lecturer not found.");
            }

            // Check if already assigned
            var exists = await _context.LecturerModules
                .FirstOrDefaultAsync(lm => lm.ModuleId == moduleId && lm.LecturerId == lecturerId);
            if (exists != null)
            {
                return BadRequest("Lecturer is already assigned to this module.");
            }

            var lecturerModule = new LecturerModule
            {
                ModuleId = moduleId,
                LecturerId = lecturerId
            };

            _context.LecturerModules.Add(lecturerModule);
            await _context.SaveChangesAsync();
            return Ok($"Lecturer {lecturer.FirstName} assigned to module {module.ModuleName}.");
        }


        //--------------------------------
        //Assign Student to Course
        //--------------------------------

        [HttpPost("courses/{courseId}/assign-student/{studentId}")]
        public async Task<IActionResult> AssignStudentToCourse(Guid courseId, Guid studentId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            var student = await _context.Students.FindAsync(studentId);
            if (course == null || student == null)
            {
                return NotFound("Course or Student not found.");
            }
            var existingEnrollment = await _context.Set<Domain.Joins.StudentCourse>()
                .FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StudentId == studentId);
            if (existingEnrollment != null)
            {
                return BadRequest("Student is already enrolled in this course.");
            }
            var studentCourse = new Domain.Joins.StudentCourse
            {
                CourseId = courseId,
                StudentId = studentId
            };
            _context.Set<Domain.Joins.StudentCourse>().Add(studentCourse);
            await _context.SaveChangesAsync();
            return Ok($"Student {student.FirstName} enrolled in course {course.CourseName}.");
        }
        //--------------------------
        //Assign Students to Modules
        //--------------------------
        [HttpPost("modules/{moduleId}/assign-student/{studentId}")]
        public async Task<IActionResult> AssignStudentToModule(Guid moduleId, Guid studentId)
        {
            var module = await _context.Modules.FindAsync(moduleId);
            var student = await _context.Students.FindAsync(studentId);
            if (module == null || student == null)
            {
                return NotFound("Module or Student not found.");
            }

            var exists = await _context.StudentModules
                .FirstOrDefaultAsync(sm => sm.ModuleId == moduleId && sm.StudentId == studentId);
            if (exists != null)
            {
                return BadRequest("Student is already assigned to this module.");
            }

            var studentModule = new StudentModule
            {
                ModuleId = moduleId,
                StudentId = studentId
            };

            _context.StudentModules.Add(studentModule);
            await _context.SaveChangesAsync();
            return Ok($"Student {student.FirstName} assigned to module {module.ModuleName}.");
        }
        //-----------------------
        //List Students in Course
        //-----------------------
        [HttpGet("courses/{courseId}/students")]
        public async Task<IActionResult> GetStudentsInCourse(Guid courseId)
        {
            var students = await _context.Set<Domain.Joins.StudentCourse>()
                 .Where(sc => sc.CourseId == courseId)
                 .Include(sc => sc.Student)
                 .Select(sc => sc.Student)
                 .ToListAsync();
            return Ok(students);
        }

        //-----------------------
        //List Courses of Student
        //-----------------------
        [HttpGet("students/{studentId}/courses")]
        public async Task<IActionResult> GetCoursesOfStudent(Guid studentId)
        {
            var courses = await _context.Set<Domain.Joins.StudentCourse>()
                .Where(sc => sc.StudentId == studentId)
                .Include(sc => sc.Course)
                .Select(sc => sc.Course)
                .ToListAsync();
            return Ok(courses);
        }

        //-----------------------
        //Search Students
        //-----------------------

        [HttpGet("students/search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search cannot be empty.");
            }
            var students = await _context.Students
                .Where(s => s.StudentNumber.Contains(query)
                || s.FirstName.Contains(query) || s.LastName.Contains(query))
                .ToListAsync();
            return Ok(students);
        }

        //------------------------
        //Search Lecturers
        //------------------------

        [HttpGet("lecturers/search")]
        public async Task<IActionResult> SearchLecturers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search cannot be empty.");
            }
            var lecturers = await _context.Lecturers
                .Where(l => l.LecturerNumber.Contains(query)
                || l.FirstName.Contains(query) || l.LastName.Contains(query))
                .ToListAsync();
            return Ok(lecturers);
        }

        public class UpdateLecturerDto
        {
            public string FirstName { get; set; } = default!;
            public string LastName { get; set; } = default!;
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; } // optional if not editable
        }

        public class UpdateStudentDto
        {
            public string FirstName { get; set; } = default!;
            public string LastName { get; set; } = default!;
            public string? Gender { get; set; }
            public DateOnly? DateOfBirth { get; set; }
            public string? HomeAddress { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
        }

        public class UpdateCourseDto
        {
            public string CourseName { get; set; } = default!;
            public string CourseCode { get; set; } = default!;
            public string? Description { get; set; }
            public Guid? LecturerId { get; set; }
        }

        public class UpdateModuleDto
        {
            public string ModuleName { get; set; } = default!;
            public string ModuleCode { get; set; } = default!;
            public string? Description { get; set; }
            public Guid? CourseId { get; set; }
        }



    }

}
