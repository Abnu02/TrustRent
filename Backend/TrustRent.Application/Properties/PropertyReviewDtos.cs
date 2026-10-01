using System.ComponentModel.DataAnnotations;
using TrustRent.Domain.Properties;

namespace TrustRent.Application.Properties;

/// <summary>Property listing details submitted by a landlord for verification.</summary>
public sealed record CreatePropertyRequest
{
    [Required, MaxLength(200)]
    public required string Address { get; init; }

    [Required, MaxLength(100)]
    public required string City { get; init; }

    [Required, StringLength(2, MinimumLength = 2)]
    public required string State { get; init; }

    [Required, MaxLength(20)]
    public required string PostalCode { get; init; }

    [Range(0, 30)]
    public int Bedrooms { get; init; }

    [Range(typeof(decimal), "0", "30")]
    public decimal Bathrooms { get; init; }

    [Range(1, 100000)]
    public int SquareFeet { get; init; }

    [Range(typeof(decimal), "0.01", "10000000")]
    public decimal MonthlyRent { get; init; }

    [MaxLength(4000)]
    public string Description { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public required string DeedFileNumber { get; init; }

    [Required, MaxLength(200)]
    public required string RecordedOwner { get; init; }

    [Required, MaxLength(100)]
    public required string ParcelId { get; init; }

    [MaxLength(200)]
    public string UtilityStatus { get; init; } = string.Empty;

    [Required, MaxLength(12)]
    public IReadOnlyList<Uri> PhotoUrls { get; init; } = [];
}

/// <summary>Administrative decision for a property verification submission.</summary>
public sealed record ReviewPropertyRequest
{
    [EnumDataType(typeof(PropertyReviewStatus))]
    public required PropertyReviewStatus Status { get; init; }

    [MaxLength(2000)]
    public string? ReviewNote { get; init; }
}

/// <summary>Property data displayed in the admin review queue and detail view.</summary>
public sealed record PropertyReviewResponse(
    Guid Id,
    Guid OwnerUserId,
    string OwnerName,
    string OwnerEmail,
    string Address,
    string City,
    string State,
    string PostalCode,
    int Bedrooms,
    decimal Bathrooms,
    int SquareFeet,
    decimal MonthlyRent,
    string Description,
    string DeedFileNumber,
    string RecordedOwner,
    string ParcelId,
    string UtilityStatus,
    IReadOnlyList<Uri> PhotoUrls,
    PropertyReviewStatus ReviewStatus,
    string? ReviewNote,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? ReviewedAt);

/// <summary>Property review counts for the admin overview.</summary>
public sealed record PropertyReviewSummaryResponse(
    int Pending,
    int Approved,
    int Rejected,
    int DocumentsRequested);

/// <summary>Audit entry for a property submission or review decision.</summary>
public sealed record PropertyReviewEventResponse(
    Guid Id,
    Guid PropertyId,
    string Action,
    string Subject,
    string ActorName,
    string ActorEmail,
    PropertyReviewStatus Status,
    string Reference,
    string? Note,
    DateTimeOffset OccurredAt);
