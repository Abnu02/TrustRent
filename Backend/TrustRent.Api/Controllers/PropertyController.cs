using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Properties.DTOs;
using TrustRent.Application.Properties.Services;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Authorize(Roles = "Landlord")]
public sealed class PropertyController(
    IPropertyService propertyService,
    ILogger<PropertyController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PropertyDto>> CreateProperty(
        [FromBody] CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetLandlordId(out var landlordId))
        {
            return Unauthorized();
        }

        var property = await propertyService.CreatePropertyAsync(landlordId, request, cancellationToken);
        logger.LogInformation("Landlord {LandlordId} created property {PropertyId}", landlordId, property.Id);
        return Created($"/api/v1/properties/{property.Id}", property);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<PropertyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PropertyDto>>> GetMyProperties(
        CancellationToken cancellationToken)
    {
        if (!TryGetLandlordId(out var landlordId))
        {
            return Unauthorized();
        }

        var properties = await propertyService.GetByLandlordAsync(landlordId, cancellationToken);
        return Ok(properties);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyDto>> UpdateProperty(
        Guid id,
        [FromBody] UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetLandlordId(out var landlordId))
        {
            return Unauthorized();
        }

        var property = await propertyService.UpdatePropertyAsync(landlordId, id, request, cancellationToken);
        if (property is null)
        {
            logger.LogWarning("Landlord {LandlordId} attempted to update unavailable property {PropertyId}", landlordId, id);
            return NotFound();
        }

        return Ok(property);
    }

    private bool TryGetLandlordId(out Guid landlordId)
    {
        var landlordIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(landlordIdClaim, out landlordId);
    }
}