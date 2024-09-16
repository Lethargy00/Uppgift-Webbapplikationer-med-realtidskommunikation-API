namespace API.Data
{
    using System;
    using System.Threading.Tasks;
    using API.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public static class AdminAccountInitializer
    {
        public static async Task CreateAdminAccount(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            string adminEmail = "admin@example.com";

            try
            {
                // Check if the user already exists
                var existingUser = await userManager.FindByEmailAsync(adminEmail);

                if (existingUser == null)
                {
                    // Create a new user
                    var user = new AppUser
                    {
                        AccountName = "ÅsaKringla",
                        UserName = adminEmail,
                        Email = adminEmail,
                    };
                    var result = await userManager.CreateAsync(user, "AdminPassword123!");

                    if (result.Succeeded)
                    {
                        // Ensure the "Admin" role exists
                        if (!await roleManager.RoleExistsAsync("Admin"))
                        {
                            var role = new IdentityRole { Name = "Admin" };
                            await roleManager.CreateAsync(role);
                        }

                        // Assign the "Admin" role to the new user
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        // Capture specific errors if account creation fails
                        throw new Exception(
                            "Failed to create admin user: " + string.Join(", ", result.Errors)
                        );
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                // Log database update exceptions
                Console.WriteLine(
                    "A database error occurred while creating the admin account: " + ex.Message
                );
                throw;
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine(
                    "An error occurred while creating the admin account: " + ex.Message
                );
                throw;
            }
        }
    }
}
