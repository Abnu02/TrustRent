using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    [EndpointSummary("Create a property listing")]
    [EndpointDescription("Creates a property owned by the authenticated landlord. New listings begin Pending and unverified.")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PropertyDto>> CreateProperty(
        [FromBody] CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetLandlordId(out var landlordId))
        {
            return Unauthorized();
        }

        var property = await propertyService.CreatePropertyAsync(landlordId, request, cancellationToken);
        logger.LogInformation("PropertyCreated {LandlordId} {PropertyId}", landlordId, property.Id);
        return CreatedAtAction(nameof(GetPropertyById), new { id = property.Id }, property);
    }

    [HttpGet("my")]
    [EndpointSummary("Get the authenticated landlord's properties")]
    [EndpointDescription("Returns a filtered, sorted, paginated property list. Supports page, pageSize (maximum 50), status, propertyType, minRent, maxRent, search, sortBy (rent/title/location), and sortDirection (asc/desc).")]
    [ProducesResponseType(typeof(PagedResult<PropertyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<PropertyDto>>> GetMyProperties(
        [FromQuery] GetMyPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        if (!TryGetLandlordId(out var landlordId))
        {
            return Unauthorized();
        }

        var properties = await propertyService.GetByLandlordAsync(landlordId, query, cancellationToken);
        logger.LogInformation(
            "PropertiesRetrieved {LandlordId} {Page} {PageSize} {TotalCount}",
            landlordId,
            properties.Page,
            properties.PageSize,
            properties.TotalCount);
        return Ok(properties);
    }

    [HttpGet("{id:guid}", Name = nameof(GetPropertyById))]
    [EndpointSummary("Get one of the authenticated landlord's properties")]
    [EndpointDescription("Returns the property when it belongs to the authenticated landlord; missing and non-owned properties both return 404.")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyDto>> GetPropertyById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetLandlordId(out var landlordId))
        {
            return Unauthorized();
        }

        var property = await propertyService.GetByIdAsync(landlordId, id, cancellationToken);
        if (property is null)
        {
            return NotFound();
        }

        return Ok(property);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a landlord-owned property")]
    [EndpointDescription("Updates listing details only. Status and verification are controlled by the platform, not supplied by landlords.")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
            return NotFound();
        }

        logger.LogInformation("PropertyUpdated {LandlordId} {PropertyId}", landlordId, id);
        return Ok(property);
    }

    private bool TryGetLandlordId(out Guid landlordId)
    {
        var landlordIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(landlordIdClaim, out landlordId);
    }
}