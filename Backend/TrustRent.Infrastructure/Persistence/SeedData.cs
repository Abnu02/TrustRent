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
}