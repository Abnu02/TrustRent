using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Domain.Users;
using TrustRent.Infrastructure.Identity;
using TrustRent.Infrastructure.Persistence;

namespace TrustRent.Infrastructure.Services;

public sealed class LandlordReviewService(ApplicationDbContext dbContext) : ILandlordReviewService
{
    public async Task<IReadOnlyList<LandlordReviewResponse>> GetPendingAsync(CancellationToken cancellationToken)
    {
        var landlords = await GetLandlords()
            .AsNoTracking()
            .Where(user => user.LandlordVerificationStatus == LandlordVerificationStatus.Pending)
            .OrderBy(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

        return landlords.Select(Map).ToArray();
    }

    public async Task<LandlordReviewResponse?> ReviewAsync(
        Guid landlordUserId,
        Guid reviewerUserId,
        LandlordVerificationStatus status,
        string? note,
        CancellationToken cancellationToken)
    {
        if (status == LandlordVerificationStatus.Pending)
        {
            throw new ArgumentOutOfRangeException(nameof(status), "A review decision cannot return a landlord to pending.");
        }

        var landlord = await GetLandlords()
            .SingleOrDefaultAsync(user => user.Id == landlordUserId, cancellationToken);
        if (landlord is null)
        {
            return null;
        }

        var reviewedAt = DateTimeOffset.UtcNow;
        landlord.LandlordVerificationStatus = status;
        landlord.IsVerified = status == LandlordVerificationStatus.Verified;
        landlord.LandlordVerificationNote = note?.Trim();
        landlord.LandlordReviewedAt = reviewedAt;
        landlord.LandlordReviewedByUserId = reviewerUserId;
        dbContext.LandlordReviewEvents.Add(new LandlordReviewEvent
        {
            Id = Guid.NewGuid(),
            LandlordUserId = landlord.Id,
            ActorUserId = reviewerUserId,
            Status = status,
            Note = landlord.LandlordVerificationNote,
            OccurredAt = reviewedAt
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(landlord);
    }

    private IQueryable<ApplicationUser> GetLandlords()
    {
        return dbContext.Users.Where(user =>
            dbContext.UserRoles.Any(userRole =>
                userRole.UserId == user.Id &&
                dbContext.Roles.Any(role => role.Id == userRole.RoleId && role.Name == Roles.Landlord)));
    }

    private static LandlordReviewResponse Map(ApplicationUser user)
    {
        return new LandlordReviewResponse(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.LandlordVerificationStatus,
            user.CreatedAt,
            user.LandlordReviewedAt,
            user.LandlordVerificationNote);
    }
}
