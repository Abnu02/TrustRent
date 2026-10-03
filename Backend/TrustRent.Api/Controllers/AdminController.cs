using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPut("properties/{id:guid}/approve")]
    public async Task<IActionResult> ApproveProperty(
        Guid id,
        CancellationToken cancellationToken)
    {
        var adminId =
            Guid.Parse(
                User.FindFirst(
                    System.Security.Claims.ClaimTypes
                        .NameIdentifier)!.Value);

        var result =
            await _service.ApprovePropertyAsync(
                adminId,
                id,
                cancellationToken);

        return Ok(result);
    }

    [HttpPut("properties/{id:guid}/reject")]
    public async Task<IActionResult> RejectProperty(
        Guid id,
        RejectRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.RejectPropertyAsync(
                id,
                cancellationToken);

        return Ok(result);
    }
}