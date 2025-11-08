using System.ComponentModel.DataAnnotations;

namespace Educore_College_LMS_Back_end.DTOs
{
    public class CourseDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CourseName { get; set; } = default!;

        [Required]
        [MaxLength(10)]
        public string CourseCode { get; set; } = default!;

        [MaxLength(200)]
        public string? Description { get; set; }

        public LecturerDto? Lecturer { get; set; }
    }
}
