using Educore_College_LMS_Back_end.Controllers;
using System.ComponentModel.DataAnnotations;
using Educore_College_LMS_Back_end.Domain.Joins;

namespace Educore_College_LMS_Back_end.DTOs
{
    public class ModuleDto
    {

        public Guid Id { get; set; }
       
        [Required]
        [MaxLength(50)]
        public string ModuleName { get; set; } = default!;

        [Required]
        [MaxLength(10)]
        public string ModuleCode { get; set; } = default!;

        public string? Description { get; set; }

        public Guid CourseId { get; set; }

    }
}
