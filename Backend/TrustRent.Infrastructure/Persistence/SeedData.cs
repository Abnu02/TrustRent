using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task EnsureLandlordRoleAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        if (!await roleManager.RoleExistsAsync(Roles.Landlord))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Landlord));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("The landlord role could not be initialized.");
            }
        }
    }
}using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { Roles.Tenant, Roles.Landlord, Roles.Admin };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var adminEmail = "admin@trustrent.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin, "AdminP@ssw0rd!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, Roles.Admin);
            }
        }
    }
}
