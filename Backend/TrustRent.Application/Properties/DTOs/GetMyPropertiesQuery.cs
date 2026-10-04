using System.ComponentModel.DataAnnotations;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.DTOs;

public sealed class GetMyPropertiesQuery : IValidatableObject
{
    [Range(1, 1_000_000)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;

    [EnumDataType(typeof(PropertyStatus))]
    public PropertyStatus? Status { get; set; }

    [EnumDataType(typeof(PropertyType))]
    public PropertyType? PropertyType { get; set; }

    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal? MinRent { get; set; }

    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal? MaxRent { get; set; }

    [StringLength(100)]
    public string? Search { get; set; }

    [RegularExpression("^(rent|title|location)$")]
    public string SortBy { get; set; } = "title";

    [RegularExpression("^(asc|desc)$")]
    public string SortDirection { get; set; } = "asc";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinRent.HasValue && MaxRent.HasValue && MinRent.Value > MaxRent.Value)
        {
            yield return new ValidationResult(
                "MinRent must be less than or equal to MaxRent.",
                [nameof(MinRent), nameof(MaxRent)]);
        }
    }
}