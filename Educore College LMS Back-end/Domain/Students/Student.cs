using Educore_College_LMS_Back_end.Controllers;
using Educore_College_LMS_Back_end.Domain.Identity;
using Educore_College_LMS_Back_end.Domain.Joins;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Educore_College_LMS_Back_end.Domain.Students
{
    public class Student
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        
        [MaxLength(20)]
        public string StudentNumber { get; set; } 

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = default!;
        [MaxLength(20)]
        public string? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? HomeAddress { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = default!;

        [Phone]
        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        public string? UserId { get; set; }
        
        public ApplicationUser? User { get; set; }

        public ICollection<StudentModule> StudentModules { get; set; }
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
        public ICollection<StudentTask> StudentTasks { get; set; } = new List<StudentTask>();
    }
}
