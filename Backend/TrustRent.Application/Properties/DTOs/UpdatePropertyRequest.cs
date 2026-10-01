using System.ComponentModel.DataAnnotations;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.DTOs;

public class UpdatePropertyRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(PropertyType))]
    public PropertyType? PropertyType { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999.99")]
    public decimal Rent { get; set; }

    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal Deposit { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Bedrooms { get; set; }

    [Range(0, 100)]
    public int Bathrooms { get; set; }
}