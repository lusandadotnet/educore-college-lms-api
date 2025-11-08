using Educore_College_LMS_Back_end.Controllers;
using Educore_College_LMS_Back_end.Domain.Courses;
using Educore_College_LMS_Back_end.Domain.Identity;
using System.ComponentModel.DataAnnotations;
using Educore_College_LMS_Back_end.Domain.Joins;

namespace Educore_College_LMS_Back_end.Domain.Lecturers
{
    public class Lecturer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ICollection<LecturerModule> LecturerModules { get; set; }

        [MaxLength(20)]
        public string LecturerNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = default!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = default!;

        [Phone]
        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();

    }
}
