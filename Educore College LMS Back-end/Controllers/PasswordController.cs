using Educore_College_LMS_Back_end.Domain.Identity;
using Educore_College_LMS_Back_end.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Educore_College_LMS_Back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public PasswordController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // POST: api/Password/Reset

        [HttpPost("forgot")]

        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if(user == null)
            {
                return NotFound("No user found with this email.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = HttpUtility.UrlEncode(token);

            // we still need to adjust this accoding to our frontend
            var resetLink = $"https://ourfrontend.com/reset-password?email={user.Email}&token={encodedToken}";

            await _emailSender.SendEmailAsync(
                user.Email!, "Password Reset Request",
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>.");

            return Ok("Password reset link has been sent to your email.");
        }

        //-----------------------
        // Reset the password
        //-----------------------

        [HttpPost("reset")]

        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if(user == null)
            {
                return NotFound("No user found with this email.");
            }
            var decodedToken = HttpUtility.UrlDecode(dto.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
            if(result.Succeeded)
            {
                return Ok("Password has been reset successfully.");
            }
            return BadRequest("Error resetting password.");
        }


        public class ForgotPasswordDto
        {
            public string Email { get; set; } = default!;
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; } = default!;
            public string Token { get; set; } = default!;
            public string NewPassword { get; set; } = default!;
        }
    }
}
