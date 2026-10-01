using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Auth.Commands;
using TrustRent.Application.Auth.Queries;
using TrustRent.Application.DTOs;
using TrustRent.Application.Properties.Commands;
using TrustRent.Application.Properties.Queries;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Tags("Admin")]
[Produces("application/json")]
public class AdminController(IMediator mediator) : ControllerBase
{
    // ==========================================
    // PROPERTY VERIFICATION (CQRS)
    // ==========================================

    [HttpGet("properties/pending")]
    [ProducesResponseType(typeof(IEnumerable<MyPropertyResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Admin views all properties awaiting verification")]
    public async Task<IActionResult> GetPendingProperties(CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPendingPropertiesQuery(), ct);
        return Ok(result);
    }

    [HttpPut("properties/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin approves and verifies a property listing")]
    public async Task<IActionResult> ApproveProperty([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new ApprovePropertyCommand(id), ct);

        return result.Match<IActionResult>(
            approved => Ok(new
            {
                message = $"Property '{approved.Title}' has been approved and is now publicly verified!",
                approved.Id,
                approved.Status,
                approved.IsVerified,
                VerifiedAt = DateTime.UtcNow
            }),
            error => NotFound(new { message = error.Message })
        );
    }

    [HttpPut("properties/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin rejects a property listing")]
    public async Task<IActionResult> RejectProperty(
        [FromRoute] Guid id,
        [FromQuery] string? reason = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new RejectPropertyCommand(id, reason), ct);

        return result.Match<IActionResult>(
            rejected => Ok(new
            {
                message = $"Property '{rejected.Title}' has been rejected.",
                rejected.Id,
                rejected.Status,
                rejected.IsVerified
            }),
            error => NotFound(new { message = error.Message })
        );
    }

    // ==========================================
    // LANDLORD VERIFICATION (CQRS)
    // ==========================================

    [HttpGet("landlords/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Admin views all landlords awaiting identity verification")]
    public async Task<IActionResult> GetPendingLandlords(CancellationToken ct = default)
    {
        var landlords = await mediator.Send(new GetPendingLandlordsQuery(), ct);
        return Ok(landlords.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.PhoneNumber,
            u.Role,
            u.IsVerified,
            u.CreatedAt
        }));
    }

    [HttpPut("landlords/{id:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin approves a landlord identity verification")]
    public async Task<IActionResult> VerifyLandlord([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new VerifyLandlordCommand(id), ct);

        return result.Match<IActionResult>(
            userId => Ok(new
            {
                message = "Landlord has been verified successfully.",
                Id = userId,
                IsVerified = true
            }),
            error => NotFound(new { message = error.Message })
        );
    }

    [HttpPut("landlords/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin rejects a landlord identity verification")]
    public async Task<IActionResult> RejectLandlord([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new RejectLandlordCommand(id), ct);

        return result.Match<IActionResult>(
            userId => Ok(new
            {
                message = "Landlord identity verification was rejected.",
                Id = userId,
                IsVerified = false
            }),
            error => NotFound(new { message = error.Message })
        );
    }
}
