using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrustRent.Domain.Users;
using TrustRent.Infrastructure.Identity;
using TrustRent.Infrastructure.Persistence;
using TrustRent.Infrastructure.Services;

namespace TrustRent.UnitTests;

public sealed class LandlordReviewServiceTests
{
    [Fact]
    public async Task GetPendingAsync_ReturnsOnlyPendingLandlordAccounts()
    {
        await using var dbContext = CreateDbContext();
        var service = new LandlordReviewService(dbContext);
        var pendingLandlord = CreateUser("Pending landlord", "pending@example.test");
        var verifiedLandlord = CreateUser("Verified landlord", "verified@example.test");
        verifiedLandlord.LandlordVerificationStatus = LandlordVerificationStatus.Verified;
        var tenant = CreateUser("Tenant", "tenant@example.test");

        await AddUsersWithRolesAsync(
            dbContext,
            (pendingLandlord, Roles.Landlord),
            (verifiedLandlord, Roles.Landlord),
            (tenant, Roles.Tenant));

        var pending = await service.GetPendingAsync(CancellationToken.None);

        var result = Assert.Single(pending);
        Assert.Equal(pendingLandlord.Id, result.Id);
        Assert.Equal(LandlordVerificationStatus.Pending, result.VerificationStatus);
    }

    [Fact]
    public async Task ReviewAsync_VerifiesLandlordAndRecordsAdminDecision()
    {
        await using var dbContext = CreateDbContext();
        var service = new LandlordReviewService(dbContext);
        var landlord = CreateUser("Pending landlord", "pending@example.test");
        var admin = CreateUser("Admin", "admin@example.test");
        await AddUsersWithRolesAsync(dbContext, (landlord, Roles.Landlord), (admin, Roles.Admin));

        var result = await service.ReviewAsync(
            landlord.Id,
            admin.Id,
            LandlordVerificationStatus.Verified,
            null,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LandlordVerificationStatus.Verified, result.VerificationStatus);
        Assert.NotNull(result.ReviewedAt);
        var savedLandlord = await dbContext.Users.SingleAsync(user => user.Id == landlord.Id);
        Assert.True(savedLandlord.IsVerified);
        Assert.Equal(admin.Id, savedLandlord.LandlordReviewedByUserId);
        Assert.Empty(await service.GetPendingAsync(CancellationToken.None));
        var reviewEvent = await dbContext.LandlordReviewEvents.SingleAsync();
        Assert.Equal(landlord.Id, reviewEvent.LandlordUserId);
        Assert.Equal(admin.Id, reviewEvent.ActorUserId);
        Assert.Equal(LandlordVerificationStatus.Verified, reviewEvent.Status);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(string fullName, string email)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FullName = fullName,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static async Task AddUsersWithRolesAsync(
        ApplicationDbContext dbContext,
        params (ApplicationUser User, string Role)[] users)
    {
        foreach (var roleName in users.Select(item => item.Role).Distinct())
        {
            var role = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };
            dbContext.Roles.Add(role);

            foreach (var (user, userRole) in users.Where(item => item.Role == roleName))
            {
                dbContext.Users.Add(user);
                dbContext.UserRoles.Add(new IdentityUserRole<Guid>
                {
                    RoleId = role.Id,
                    UserId = user.Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }
}
