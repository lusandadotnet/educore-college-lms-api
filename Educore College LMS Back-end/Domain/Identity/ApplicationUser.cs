using Microsoft.AspNetCore.Identity;
namespace Educore_College_LMS_Back_end.Domain.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? StudentId { get; set; }
        public Guid? LecturerId { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
