using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Domain.Properties;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Infrastructure.Identity;
using TrustRent.Infrastructure.Data;
using TrustRent.Infrastructure.Persistence;
using TrustRent.Domain.Users;

namespace TrustRent.Infrastructure.Services;

public sealed class PropertyReviewService(
    ApplicationDbContext dbContext,
    TrustRentDbContext propertyContext) : IPropertyReviewService
{
    public async Task<IReadOnlyList<PropertyReviewResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var listings = await dbContext.PropertyListings
            .AsNoTracking()
            .Join(dbContext.Users, listing => listing.OwnerUserId, owner => owner.Id, (listing, owner) => new { listing, owner })
            .OrderBy(item => item.listing.ReviewStatus)
            .ThenBy(item => item.listing.SubmittedAt)
            .ToListAsync(cancellationToken);

        var legacyProperties = await propertyContext.Properties
            .AsNoTracking()
            .OrderBy(property => property.Status)
            .ThenBy(property => property.SubmittedAt)
            .ToListAsync(cancellationToken);
        var ownerIds = legacyProperties.Select(property => property.LandlordId).Distinct().ToArray();
        var owners = await dbContext.Users
            .AsNoTracking()
            .Where(owner => ownerIds.Contains(owner.Id))
            .ToDictionaryAsync(owner => owner.Id, cancellationToken);

        var legacyListings = legacyProperties.Select(property =>
            MapLegacy(property, GetOwner(owners, property.LandlordId)));
        return listings.Select(item => Map(item.listing, item.owner))
            .Concat(legacyListings)
            .OrderBy(listing => listing.ReviewStatus)
            .ThenBy(listing => listing.SubmittedAt)
            .ToArray();
    }

    public async Task<PropertyReviewResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var listing = await dbContext.PropertyListings
            .AsNoTracking()
            .Where(property => property.Id == id)
            .Join(dbContext.Users, property => property.OwnerUserId, owner => owner.Id, (property, owner) => new { property, owner })
            .SingleOrDefaultAsync(cancellationToken);

        if (listing is not null)
        {
            return Map(listing.property, listing.owner);
        }

        var legacyProperty = await propertyContext.Properties
            .AsNoTracking()
            .SingleOrDefaultAsync(property => property.Id == id, cancellationToken);
        if (legacyProperty is null)
        {
            return null;
        }

        var owner = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == legacyProperty.LandlordId, cancellationToken)
            ?? throw new InvalidOperationException($"Owner {legacyProperty.LandlordId} for property {id} was not found.");
        return MapLegacy(legacyProperty, owner);
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
            var legacyProperty = await propertyContext.Properties
                .SingleOrDefaultAsync(property => property.Id == id, cancellationToken);
            if (legacyProperty is null)
            {
                return null;
            }

            var legacyOwner = await dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == legacyProperty.LandlordId, cancellationToken)
                ?? throw new InvalidOperationException($"Owner {legacyProperty.LandlordId} for property {id} was not found.");
            legacyProperty.Status = request.Status switch
            {
                PropertyReviewStatus.Pending => PropertyStatus.Pending,
                PropertyReviewStatus.Approved => PropertyStatus.Approved,
                PropertyReviewStatus.Rejected => PropertyStatus.Rejected,
                PropertyReviewStatus.DocumentsRequested => PropertyStatus.DocumentsRequested,
                _ => throw new ArgumentOutOfRangeException(nameof(request.Status), request.Status, "Unknown property review status.")
            };
            legacyProperty.IsVerified = request.Status == PropertyReviewStatus.Approved;
            legacyProperty.ReviewNote = request.ReviewNote?.Trim();
            legacyProperty.ReviewedByUserId = reviewerUserId;
            legacyProperty.ReviewedAt = DateTimeOffset.UtcNow;
            propertyContext.PropertyReviewEvents.Add(new LegacyPropertyReviewEvent
            {
                Id = Guid.NewGuid(),
                PropertyId = legacyProperty.Id,
                ActorUserId = reviewerUserId,
                Status = legacyProperty.Status,
                Note = legacyProperty.ReviewNote,
                OccurredAt = legacyProperty.ReviewedAt.Value
            });
            await propertyContext.SaveChangesAsync(cancellationToken);
            return MapLegacy(legacyProperty, legacyOwner);
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

        var legacyCounts = await propertyContext.Properties
            .AsNoTracking()
            .GroupBy(property => property.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.Status, group => group.Count, cancellationToken);

        return new PropertyReviewSummaryResponse(
            counts.GetValueOrDefault(PropertyReviewStatus.Pending) + legacyCounts.GetValueOrDefault(PropertyStatus.Pending),
            counts.GetValueOrDefault(PropertyReviewStatus.Approved) + legacyCounts.GetValueOrDefault(PropertyStatus.Approved),
            counts.GetValueOrDefault(PropertyReviewStatus.Rejected) + legacyCounts.GetValueOrDefault(PropertyStatus.Rejected),
            counts.GetValueOrDefault(PropertyReviewStatus.DocumentsRequested) + legacyCounts.GetValueOrDefault(PropertyStatus.DocumentsRequested));
    }

    public async Task<IReadOnlyList<PropertyReviewEventResponse>> GetAuditLogAsync(CancellationToken cancellationToken)
    {
        var propertyEvents = await dbContext.PropertyReviewEvents
            .AsNoTracking()
            .Join(dbContext.PropertyListings, reviewEvent => reviewEvent.PropertyId, listing => listing.Id,
                (reviewEvent, listing) => new { reviewEvent, listing })
            .Join(dbContext.Users, item => item.reviewEvent.ActorUserId, actor => actor.Id,
                (item, actor) => new { item.reviewEvent, item.listing, actor })
            .OrderByDescending(item => item.reviewEvent.OccurredAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var landlordEvents = await dbContext.LandlordReviewEvents
            .AsNoTracking()
            .Join(dbContext.Users, reviewEvent => reviewEvent.LandlordUserId, landlord => landlord.Id,
                (reviewEvent, landlord) => new { reviewEvent, landlord })
            .Join(dbContext.Users, item => item.reviewEvent.ActorUserId, actor => actor.Id,
                (item, actor) => new { item.reviewEvent, item.landlord, actor })
            .OrderByDescending(item => item.reviewEvent.OccurredAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var legacyPropertyEvents = await propertyContext.PropertyReviewEvents
            .AsNoTracking()
            .Join(propertyContext.Properties, reviewEvent => reviewEvent.PropertyId, property => property.Id,
                (reviewEvent, property) => new { reviewEvent, property })
            .OrderByDescending(item => item.reviewEvent.OccurredAt)
            .Take(50)
            .ToListAsync(cancellationToken);
        var legacyActorIds = legacyPropertyEvents
            .Select(item => item.reviewEvent.ActorUserId)
            .Distinct()
            .ToArray();
        var legacyActors = await dbContext.Users
            .AsNoTracking()
            .Where(actor => legacyActorIds.Contains(actor.Id))
            .ToDictionaryAsync(actor => actor.Id, cancellationToken);

        var propertyEntries = propertyEvents.Select(item => new PropertyReviewEventResponse(
            item.reviewEvent.Id,
            item.reviewEvent.PropertyId,
            GetAction(item.reviewEvent.Status),
            item.listing.Address,
            item.actor.FullName,
            item.actor.Email ?? string.Empty,
            item.reviewEvent.Status.ToString(),
            item.listing.DeedFileNumber,
            item.reviewEvent.Note,
            item.reviewEvent.OccurredAt));
        var landlordEntries = landlordEvents.Select(item => new PropertyReviewEventResponse(
            item.reviewEvent.Id,
            null,
            item.reviewEvent.Status == LandlordVerificationStatus.Verified
                ? "Landlord verified"
                : "Landlord verification rejected",
            item.landlord.FullName,
            item.actor.FullName,
            item.actor.Email ?? string.Empty,
            item.reviewEvent.Status.ToString(),
            item.landlord.Id.ToString(),
            item.reviewEvent.Note,
            item.reviewEvent.OccurredAt));
        var legacyPropertyEntries = legacyPropertyEvents.Select(item => new PropertyReviewEventResponse(
            item.reviewEvent.Id,
            item.reviewEvent.PropertyId,
            GetAction(item.reviewEvent.Status switch
            {
                PropertyStatus.Pending => PropertyReviewStatus.Pending,
                PropertyStatus.Approved => PropertyReviewStatus.Approved,
                PropertyStatus.Rejected => PropertyReviewStatus.Rejected,
                PropertyStatus.DocumentsRequested => PropertyReviewStatus.DocumentsRequested,
                _ => throw new ArgumentOutOfRangeException(nameof(item.reviewEvent.Status), item.reviewEvent.Status, "Unknown property status.")
            }),
            item.property.Title,
            GetOwner(legacyActors, item.reviewEvent.ActorUserId).FullName,
            GetOwner(legacyActors, item.reviewEvent.ActorUserId).Email ?? string.Empty,
            item.reviewEvent.Status.ToString(),
            item.property.Id.ToString(),
            item.reviewEvent.Note,
            item.reviewEvent.OccurredAt));

        return propertyEntries.Concat(legacyPropertyEntries).Concat(landlordEntries)
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(50)
            .ToArray();
    }

    private static string GetAction(PropertyReviewStatus status) => status switch
    {
        PropertyReviewStatus.Pending => "Property submitted",
        PropertyReviewStatus.Approved => "Property approved",
        PropertyReviewStatus.Rejected => "Property rejected",
        PropertyReviewStatus.DocumentsRequested => "Documents requested",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown property review status.")
    };

    private static ApplicationUser GetOwner(IReadOnlyDictionary<Guid, ApplicationUser> owners, Guid ownerId)
    {
        return owners.TryGetValue(ownerId, out var owner)
            ? owner
            : throw new InvalidOperationException($"Owner {ownerId} for a property listing was not found.");
    }

    private static PropertyReviewResponse MapLegacy(Property property, ApplicationUser owner)
    {
        var reviewStatus = property.Status switch
        {
            PropertyStatus.Pending => PropertyReviewStatus.Pending,
            PropertyStatus.Approved => PropertyReviewStatus.Approved,
            PropertyStatus.Rejected => PropertyReviewStatus.Rejected,
            PropertyStatus.DocumentsRequested => PropertyReviewStatus.DocumentsRequested,
            _ => throw new ArgumentOutOfRangeException(nameof(property.Status), property.Status, "Unknown property status.")
        };

        return new PropertyReviewResponse(
            property.Id,
            property.LandlordId,
            owner.FullName,
            owner.Email ?? string.Empty,
            property.Title,
            property.Location,
            string.Empty,
            string.Empty,
            property.Bedrooms,
            property.Bathrooms,
            0,
            property.Rent,
            property.Description,
            string.Empty,
            owner.FullName,
            string.Empty,
            string.Empty,
            [],
            reviewStatus,
            property.ReviewNote,
            property.SubmittedAt,
            property.ReviewedAt);
    }

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
