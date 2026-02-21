using AlmavivaSlotChecker.Data;
using Microsoft.AspNetCore.Identity;

namespace AlmavivaSlotChecker.Services.Implementations;

public class IdentitySeedService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    private static readonly string[] Roles = ["Admin", "User"];

    public async Task SeedAsync()
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@local";
        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, "Admin123!");
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
