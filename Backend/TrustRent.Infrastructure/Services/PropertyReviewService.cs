using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Domain.Properties;
using TrustRent.Infrastructure.Identity;
using TrustRent.Infrastructure.Persistence;

namespace TrustRent.Infrastructure.Services;

public sealed class PropertyReviewService(ApplicationDbContext dbContext) : IPropertyReviewService
{
    public async Task<IReadOnlyList<PropertyReviewResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var listings = await dbContext.PropertyListings
            .AsNoTracking()
            .Join(dbContext.Users, listing => listing.OwnerUserId, owner => owner.Id, (listing, owner) => new { listing, owner })
            .OrderBy(item => item.listing.ReviewStatus)
            .ThenBy(item => item.listing.SubmittedAt)
            .ToListAsync(cancellationToken);

        return listings.Select(item => Map(item.listing, item.owner)).ToArray();
    }

    public async Task<PropertyReviewResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var listing = await dbContext.PropertyListings
            .AsNoTracking()
            .Where(property => property.Id == id)
            .Join(dbContext.Users, property => property.OwnerUserId, owner => owner.Id, (property, owner) => new { property, owner })
            .SingleOrDefaultAsync(cancellationToken);

        return listing is null ? null : Map(listing.property, listing.owner);
    }

    public async Task<PropertyReviewResponse> CreateAsync(
        Guid ownerUserId,
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var owner = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == ownerUserId, cancellationToken)
            ?? throw new KeyNotFoundException("The landlord account was not found.");

        var listing = new PropertyListing
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim().ToUpperInvariant(),
            PostalCode = request.PostalCode.Trim(),
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            SquareFeet = request.SquareFeet,
            MonthlyRent = request.MonthlyRent,
            Description = request.Description?.Trim() ?? string.Empty,
            DeedFileNumber = request.DeedFileNumber.Trim(),
            RecordedOwner = request.RecordedOwner.Trim(),
            ParcelId = request.ParcelId.Trim(),
            UtilityStatus = request.UtilityStatus?.Trim() ?? string.Empty,
            PhotoUrls = request.PhotoUrls.Select(uri => uri.ToString()).ToArray(),
            ReviewStatus = PropertyReviewStatus.Pending,
            SubmittedAt = DateTimeOffset.UtcNow
        };

        dbContext.PropertyListings.Add(listing);
        dbContext.PropertyReviewEvents.Add(new PropertyReviewEvent
        {
            Id = Guid.NewGuid(),
            PropertyId = listing.Id,
            ActorUserId = ownerUserId,
            Status = PropertyReviewStatus.Pending,
            OccurredAt = listing.SubmittedAt
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(listing, owner);
    }

    public async Task<PropertyReviewResponse?> ReviewAsync(
        Guid id,
        Guid reviewerUserId,
        ReviewPropertyRequest request,
        CancellationToken cancellationToken)
    {
        var listing = await dbContext.PropertyListings
            .SingleOrDefaultAsync(property => property.Id == id, cancellationToken);

        if (listing is null)
        {
            return null;
        }

        var owner = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == listing.OwnerUserId, cancellationToken)
            ?? throw new InvalidOperationException("A property listing owner was not found.");

        listing.ReviewStatus = request.Status;
        listing.ReviewNote = request.ReviewNote?.Trim();
        listing.ReviewedByUserId = reviewerUserId;
        listing.ReviewedAt = DateTimeOffset.UtcNow;
        dbContext.PropertyReviewEvents.Add(new PropertyReviewEvent
        {
            Id = Guid.NewGuid(),
            PropertyId = listing.Id,
            ActorUserId = reviewerUserId,
            Status = request.Status,
            Note = listing.ReviewNote,
            OccurredAt = listing.ReviewedAt.Value
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(listing, owner);
    }

    public async Task<PropertyReviewSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var counts = await dbContext.PropertyListings
            .AsNoTracking()
            .GroupBy(listing => listing.ReviewStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.Status, group => group.Count, cancellationToken);

        return new PropertyReviewSummaryResponse(
            counts.GetValueOrDefault(PropertyReviewStatus.Pending),
            counts.GetValueOrDefault(PropertyReviewStatus.Approved),
            counts.GetValueOrDefault(PropertyReviewStatus.Rejected),
            counts.GetValueOrDefault(PropertyReviewStatus.DocumentsRequested));
    }

    public async Task<IReadOnlyList<PropertyReviewEventResponse>> GetAuditLogAsync(CancellationToken cancellationToken)
    {
        var events = await dbContext.PropertyReviewEvents
            .AsNoTracking()
            .Join(dbContext.PropertyListings, reviewEvent => reviewEvent.PropertyId, listing => listing.Id,
                (reviewEvent, listing) => new { reviewEvent, listing })
            .Join(dbContext.Users, item => item.reviewEvent.ActorUserId, actor => actor.Id,
                (item, actor) => new { item.reviewEvent, item.listing, actor })
            .OrderByDescending(item => item.reviewEvent.OccurredAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return events.Select(item => new PropertyReviewEventResponse(
            item.reviewEvent.Id,
            item.reviewEvent.PropertyId,
            GetAction(item.reviewEvent.Status),
            item.listing.Address,
            item.actor.FullName,
            item.actor.Email ?? string.Empty,
            item.reviewEvent.Status,
            item.listing.DeedFileNumber,
            item.reviewEvent.Note,
            item.reviewEvent.OccurredAt)).ToArray();
    }

    private static string GetAction(PropertyReviewStatus status) => status switch
    {
        PropertyReviewStatus.Pending => "Property submitted",
        PropertyReviewStatus.Approved => "Property approved",
        PropertyReviewStatus.Rejected => "Property rejected",
        PropertyReviewStatus.DocumentsRequested => "Documents requested",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown property review status.")
    };

    private static PropertyReviewResponse Map(PropertyListing listing, ApplicationUser owner)
    {
        return new PropertyReviewResponse(
            listing.Id,
            listing.OwnerUserId,
            owner.FullName,
            owner.Email ?? string.Empty,
            listing.Address,
            listing.City,
            listing.State,
            listing.PostalCode,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.SquareFeet,
            listing.MonthlyRent,
            listing.Description,
            listing.DeedFileNumber,
            listing.RecordedOwner,
            listing.ParcelId,
            listing.UtilityStatus,
            listing.PhotoUrls.Select(uri => new Uri(uri)).ToArray(),
            listing.ReviewStatus,
            listing.ReviewNote,
            listing.SubmittedAt,
            listing.ReviewedAt);
    }
}
