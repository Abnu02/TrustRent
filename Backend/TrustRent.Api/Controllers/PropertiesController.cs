using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Application.Exceptions;
using TrustRent.Application.Interfaces;
using TrustRent.Domain.Repositories;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Tags("Properties")]
[Produces("application/json")]
public class PropertiesController(
    ILandlordPropertyService propertyService,
    IPropertyRepository propertyRepository) : ControllerBase
{
    private Guid GetCurrentLandlordId()
    {
        if (Request.Headers.TryGetValue("X-Landlord-Id", out var customId) &&
            Guid.TryParse(customId, out var parsedGuid))
        {
            return parsedGuid;
        }

        // Default seeded Landlord (Abreham Bekele) for hackathon & development
        return Guid.Parse("8c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f");
    }

    /// <summary>
    /// Step 1: Landlord registers their account and profile.
    /// </summary>
    [HttpPost("landlord/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Register a new Landlord profile")]
    public async Task<IActionResult> RegisterLandlord([FromBody] RegisterLandlordRequest request)
    {
        try
        {
            var user = await propertyService.RegisterLandlordAsync(request);
            return StatusCode(StatusCodes.Status201Created, new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.IsVerified,
                message = "Landlord registered successfully. You can now create property listings."
            });
        }
        catch (PropertyValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Step 2 & 3: Landlord creates a new property and submits it for Admin verification.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PropertyCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Landlord creates and submits a property for verification")]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        try
        {
            var landlordId = GetCurrentLandlordId();
            var result = await propertyService.CreatePropertyAsync(landlordId, request);
            return CreatedAtAction(nameof(GetPropertyById), new { id = result.Id }, result);
        }
        catch (PropertyValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Property Data",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Step 4 (Part A): Landlord views their properties with Server-Side Pagination, Filtering & Search.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResponse<MyPropertyResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Get paginated properties owned by the current landlord")]
    public async Task<IActionResult> GetMyProperties(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool sortDescending = true)
    {
        var landlordId = GetCurrentLandlordId();
        var pagedRequest = new PagedRequest(page, pageSize);

        var pagedResponse = await propertyService.GetMyPropertiesPagedAsync(
            landlordId,
            pagedRequest,
            status,
            search,
            sortBy,
            sortDescending
        );

        return Ok(pagedResponse);
    }

    /// <summary>
    /// Step 4 (Part B): Landlord views their portfolio summary metrics.
    /// </summary>
    [HttpGet("my/stats")]
    [ProducesResponseType(typeof(LandlordStatsDto), StatusCodes.Status200OK)]
    [EndpointSummary("Get landlord portfolio statistics and verification metrics")]
    public async Task<IActionResult> GetMyStats()
    {
        var landlordId = GetCurrentLandlordId();
        var stats = await propertyService.GetLandlordStatsAsync(landlordId);
        return Ok(stats);
    }

    /// <summary>
    /// Tenant or Landlord retrieves full property details with verified landlord contact info.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PropertyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get property details with verified landlord contact")]
    public async Task<IActionResult> GetPropertyById([FromRoute] Guid id)
    {
        var property = await propertyService.GetPropertyByIdAsync(id);
        if (property == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Property Not Found",
                Detail = $"Property with ID '{id}' was not found in the system.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(property);
    }

    /// <summary>
    /// Landlord updates their own property listing.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PropertyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Landlord updates a property listing")]
    public async Task<IActionResult> UpdateProperty([FromRoute] Guid id, [FromBody] UpdatePropertyRequest request)
    {
        try
        {
            var landlordId = GetCurrentLandlordId();
            var updated = await propertyService.UpdatePropertyAsync(landlordId, id, request);
            return Ok(updated);
        }
        catch (UnauthorizedPropertyAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Access Forbidden",
                Detail = ex.Message,
                Status = StatusCodes.Status403Forbidden
            });
        }
        catch (PropertyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Property Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (PropertyValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
