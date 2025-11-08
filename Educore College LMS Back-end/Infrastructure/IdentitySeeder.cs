using Educore_College_LMS_Back_end.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;

namespace Educore_College_LMS_Back_end.Infrastructure
{
    public class IdentitySeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole>roleManager, UserManager<ApplicationUser>userManager)
        {
   
            string[] roles = { "Admin", "Lecturer", "Student" };
            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
                
                

            }

            string adminEmail = "";
            string adminPassword = "";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if(adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if(!result.Succeeded)
                {
                    throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

        }
    }
}
