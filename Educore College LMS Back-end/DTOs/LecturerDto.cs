using Educore_College_LMS_Back_end.Controllers;
using System.ComponentModel.DataAnnotations;
using Educore_College_LMS_Back_end.Domain.Joins;

namespace Educore_College_LMS_Back_end.DTOs
{
    public class LecturerDto
    {
       
        public Guid Id { get; set; }

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

        
    }
}
