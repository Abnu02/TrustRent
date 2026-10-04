using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.DTOs.Auth;
using TrustRent.Application.DTOs.Verification;
using TrustRent.Application.Services;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminVerificationService _service;

    public AdminController(
        IAdminVerificationService service)
    {
        _service = service;
    }

    [HttpGet("landlords")]
    public async Task<IActionResult> GetLandlords(
        CancellationToken cancellationToken)
    {
        var landlords = await _service.GetLandlordsAsync(cancellationToken);
        return Ok(landlords);
    }

    [HttpGet("landlords/{id:guid}")]
    public async Task<IActionResult> GetLandlord(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var landlord = await _service.GetLandlordAsync(id, cancellationToken);
            return landlord is null ? NotFound() : Ok(landlord);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("landlords")]
    public async Task<IActionResult> CreateLandlord(
        CreateLandlordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var landlord = await _service.CreateLandlordAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetLandlord),
                new { id = landlord.Id },
                landlord);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("landlords/{id:guid}")]
    public async Task<IActionResult> UpdateLandlord(
        Guid id,
        UpdateLandlordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var landlord = await _service.UpdateLandlordAsync(
                id,
                request,
                cancellationToken);

            return Ok(landlord);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("landlords/{id:guid}/active")]
    public async Task<IActionResult> SetLandlordActive(
        Guid id,
        SetLandlordActiveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.SetLandlordActiveAsync(
                id,
                request.IsActive,
                cancellationToken);

            return Ok(new { landlordId = id, request.IsActive });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("landlords/{id:guid}")]
    public async Task<IActionResult> DeactivateLandlord(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.SetLandlordActiveAsync(
                id, false, cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("landlords/pending")]
    public async Task<IActionResult> GetPendingLandlords(
        CancellationToken cancellationToken)
    {
        var result =
            await _service.GetPendingLandlordsAsync(
                cancellationToken);

        return Ok(result);
    }

    [HttpPut("landlords/{id:guid}/verify")]
    public async Task<IActionResult> VerifyLandlord(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _service.VerifyLandlordAsync(
            id,
            cancellationToken);

        return Ok(new
        {
            message = "Landlord verified successfully.",
            landlordId = id,
            isVerified = true
        });
    }

    [HttpPut("landlords/{id:guid}/reject")]
    public async Task<IActionResult> RejectLandlord(
        Guid id,
        RejectRequest request,
        CancellationToken cancellationToken)
    {
        await _service.RejectLandlordAsync(
            id,
            request.Reason,
            cancellationToken);

        return Ok(new
        {
            message = "Landlord rejected.",
            landlordId = id,
            isVerified = false
        });
    }

    [HttpGet("properties/pending")]
    public async Task<IActionResult> GetPendingProperties(
        CancellationToken cancellationToken)
    {
        var result =
            await _service.GetPendingPropertiesAsync(
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("properties")]
    public async Task<IActionResult> GetAllProperties(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllPropertiesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut("properties/{id:guid}/approve")]
    public async Task<IActionResult> ApproveProperty(
        Guid id,
        CancellationToken cancellationToken)
    {
        var adminIdValue = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(adminIdValue, out var adminId))
        {
            return Unauthorized(new { message = "Invalid admin identity." });
        }

        try
        {
            var result = await _service.ApprovePropertyAsync(
                adminId,
                id,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("properties/{id:guid}/reject")]
    public async Task<IActionResult> RejectProperty(
        Guid id,
        RejectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectPropertyAsync(
                id,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}