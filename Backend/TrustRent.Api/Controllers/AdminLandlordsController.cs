using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Domain.Users;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/admin/landlords")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminLandlordsController(ILandlordReviewService landlordReviewService) : ControllerBase
{
    [HttpGet("pending")]
    [ProducesResponseType<IReadOnlyList<LandlordReviewResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LandlordReviewResponse>>> GetPending(CancellationToken cancellationToken)
    {
        return Ok(await landlordReviewService.GetPendingAsync(cancellationToken));
    }

    [HttpPut("{id:guid}/verify")]
    [ProducesResponseType<LandlordReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<LandlordReviewResponse>> Verify(Guid id, CancellationToken cancellationToken)
    {
        return Review(id, LandlordVerificationStatus.Verified, null, cancellationToken);
    }

    [HttpPut("{id:guid}/reject")]
    [ProducesResponseType<LandlordReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<LandlordReviewResponse>> Reject(
        Guid id,
        ReviewLandlordRequest request,
        CancellationToken cancellationToken)
    {
        return Review(id, LandlordVerificationStatus.Rejected, request.Reason, cancellationToken);
    }

    private async Task<ActionResult<LandlordReviewResponse>> Review(
        Guid id,
        LandlordVerificationStatus status,
        string? note,
        CancellationToken cancellationToken)
    {
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(reviewerId, out var reviewerUserId))
        {
            return Unauthorized();
        }

        var landlord = await landlordReviewService.ReviewAsync(
            id, reviewerUserId, status, note, cancellationToken);
        return landlord is null ? NotFound() : Ok(landlord);
    }
}
