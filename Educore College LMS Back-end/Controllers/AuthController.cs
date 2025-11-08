using Educore_College_LMS_Back_end.Domain.Identity;
using Educore_College_LMS_Back_end.Domain.Students; // make sure you can access Student entity
using Educore_College_LMS_Back_end.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // needed for querying Student
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Educore_College_LMS_Back_end.Domain.Lecturers;

namespace Educore_College_LMS_Back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context; // add DbContext to query Students

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        // -------------------------
        // Login Endpoint
        // -------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Find user
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            // 2. Check password
            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            // 3. Check if user must change password
            if (user.MustChangePassword)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                return Ok(new
                {
                    mustChangePassword = true,
                    message = "You must change your temporary password before continuing.",
                    resetToken,
                    email = user.Email
                });
            }

            // 4. Get roles
            var roles = await _userManager.GetRolesAsync(user);

            // 5. Handle profile
            Student? student = null;
            Lecturer? lecturer = null;

            if (roles.Contains("Student"))
            {
                student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
               

                if (student == null)
                    return BadRequest("Student record not found.");
                


            }
            if (roles.Contains("Lecturer"))
            {
               
                lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);

               
                if (lecturer == null)
                    return BadRequest("Lecturer record not found.");


            }

            // 6. Generate token (for student use student.Id, else use user.Id)
            var (token, expiration) = GenerateJwtToken(user, roles, student, lecturer);

            // 7. Generate refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token,
                expiration,
                refreshToken,
                roles,
                profile = user.StudentId.HasValue? "Student" : "Lecturer",  // include student profile only if applicable
                
                // include lecturer profile only if applicable

            });
        }


        //========================
        //Change Password Endpoint
        //========================
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("New password and confirmation do not match.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid user." });

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password reset successfully." });
        }

        //------------------------
        // Refresh Token Endpoint
        //------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null) return BadRequest("Invalid token");

            var studentId = principal.FindFirstValue("sub"); // student id only if a student
            var lecturerId = principal.FindFirstValue("sub"); // lecturer id only if a lecturer

            ApplicationUser user;
            Student? student = null;
            Lecturer? lecturer = null;

            if (!string.IsNullOrEmpty(studentId) || !string.IsNullOrEmpty(lecturerId))
            {
                // Student flow
                student = await _context.Students.FirstOrDefaultAsync(s => s.Id.ToString() == studentId);
                lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Id.ToString() == lecturerId);
                if (student == null) return BadRequest("Student not found");
                 else if (lecturer == null) return BadRequest("Lecturer not found");

                user = await _userManager.FindByIdAsync(student.UserId);
                user = await _userManager.FindByIdAsync(lecturer.UserId);
            }
            else
            {
                // Non-student (admin) flow: use the user ID from the claims
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                user = await _userManager.FindByIdAsync(userId);
            }

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return BadRequest("Invalid refresh token");

            var roles = await _userManager.GetRolesAsync(user);
            var (newToken, expiration) = GenerateJwtToken(user, roles, student, lecturer);

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token = newToken,
                expiration,
                refreshToken = newRefreshToken,
                roles
            });
        }


        // -------------------------
        // Helper: Generate JWT for Student
        // -------------------------
        private (string token, DateTime expiration) GenerateJwtToken(
             ApplicationUser user,
             IList<string> roles,
             Student? student = null, Lecturer? lecturer = null)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim("email", user.Email ?? "")
                };

            if (student != null)
            {
                // if student, put their student ID in sub claim
                claims.Add(new Claim("sub", student.Id.ToString()));

            }
            if (lecturer != null)
            {
                claims.Add(new Claim("sub", lecturer.Id.ToString()));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiration,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }


        // -------------------------
        // Generate Secure Refresh Token
        // -------------------------
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // -------------------------
        // Extract Principal from Expired Token
        // -------------------------
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = Environment.GetEnvironmentVariable("JWT_KEY")
                      ?? _configuration.GetSection("Jwt").GetValue<string>("Key");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

    // -------------------------
    // DTOs
    // -------------------------
    public class LoginDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
