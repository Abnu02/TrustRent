using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Auth.Commands;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Application.Properties.Commands;
using TrustRent.Application.Properties.Queries;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Tags("Properties")]
[Produces("application/json")]
public class PropertiesController(IMediator mediator) : ControllerBase
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
    public async Task<IActionResult> RegisterLandlord(
        [FromBody] RegisterLandlordRequest request,
        CancellationToken ct)
    {
        var command = new RegisterLandlordCommand(
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.Password
        );

        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            user => StatusCode(StatusCodes.Status201Created, new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                message = "Landlord registered successfully. You can now create property listings."
            }),
            error => BadRequest(new ProblemDetails
            {
                Title = error.Code == "conflict" ? "Conflict" : "Validation Error",
                Detail = error.Message,
                Status = error.Code == "conflict" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest
            })
        );
    }

    /// <summary>
    /// Step 2: Landlord creates a new property.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PropertyCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Landlord creates and submits a property for verification")]
    public async Task<IActionResult> CreateProperty(
        [FromBody] CreatePropertyRequest request,
        CancellationToken ct)
    {
        var landlordId = GetCurrentLandlordId();
        var command = new CreatePropertyCommand(
            landlordId,
            request.Title,
            request.Description,
            request.PropertyType,
            request.Rent,
            request.Deposit,
            request.Location,
            request.Bedrooms,
            request.Bathrooms
        );

        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            created => CreatedAtAction(nameof(GetPropertyById), new { id = created.Id }, created),
            error => error.Code switch
            {
                "landlord_not_found" => NotFound(new ProblemDetails
                {
                    Title = "Landlord Not Found",
                    Detail = error.Message,
                    Status = StatusCodes.Status404NotFound
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = error.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            }
        );
    }

    /// <summary>
    /// Step 3: Landlord submits an existing property for verification.
    /// </summary>
    [HttpPost("{id:guid}/submit-verification")]
    [ProducesResponseType(typeof(PropertyCreatedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Submit an existing property listing for auditor verification")]
    public async Task<IActionResult> SubmitForVerification(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var landlordId = GetCurrentLandlordId();
        var command = new SubmitPropertyForVerificationCommand(id, landlordId);

        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            updated => Ok(updated),
            error => error.Code switch
            {
                "property_not_found" => NotFound(new ProblemDetails
                {
                    Title = "Property Not Found",
                    Detail = error.Message,
                    Status = StatusCodes.Status404NotFound
                }),
                "unauthorized" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Title = "Access Forbidden",
                    Detail = error.Message,
                    Status = StatusCodes.Status403Forbidden
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Invalid Status",
                    Detail = error.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            }
        );
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
        [FromQuery] bool sortDescending = true,
        CancellationToken ct = default)
    {
        var landlordId = GetCurrentLandlordId();
        var query = new GetLandlordPropertiesQuery(
            landlordId,
            page,
            pageSize,
            status,
            search,
            sortBy,
            sortDescending
        );

        var pagedResponse = await mediator.Send(query, ct);
        return Ok(pagedResponse);
    }

    /// <summary>
    /// Step 4 (Part B): Landlord views their portfolio summary metrics.
    /// </summary>
    [HttpGet("my/stats")]
    [ProducesResponseType(typeof(LandlordStatsDto), StatusCodes.Status200OK)]
    [EndpointSummary("Get landlord portfolio statistics and verification metrics")]
    public async Task<IActionResult> GetMyStats(CancellationToken ct = default)
    {
        var landlordId = GetCurrentLandlordId();
        var query = new GetLandlordStatsQuery(landlordId);
        var stats = await mediator.Send(query, ct);
        return Ok(stats);
    }

    /// <summary>
    /// Tenant or Landlord retrieves full property details with verified landlord contact info.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PropertyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get property details with verified landlord contact")]
    public async Task<IActionResult> GetPropertyById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var query = new GetPropertyByIdQuery(id);
        var result = await mediator.Send(query, ct);

        return result.Match<IActionResult>(
            property => Ok(property),
            error => NotFound(new ProblemDetails
            {
                Title = "Property Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            })
        );
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
    public async Task<IActionResult> UpdateProperty(
        [FromRoute] Guid id,
        [FromBody] UpdatePropertyRequest request,
        CancellationToken ct = default)
    {
        var landlordId = GetCurrentLandlordId();
        var command = new UpdatePropertyCommand(
            id,
            landlordId,
            request.Title,
            request.Description,
            request.PropertyType,
            request.Rent,
            request.Deposit,
            request.Location,
            request.Bedrooms,
            request.Bathrooms
        );

        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            updated => Ok(updated),
            error => error.Code switch
            {
                "unauthorized" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Title = "Access Forbidden",
                    Detail = error.Message,
                    Status = StatusCodes.Status403Forbidden
                }),
                "property_not_found" => NotFound(new ProblemDetails
                {
                    Title = "Property Not Found",
                    Detail = error.Message,
                    Status = StatusCodes.Status404NotFound
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = error.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            }
        );
    }
}
