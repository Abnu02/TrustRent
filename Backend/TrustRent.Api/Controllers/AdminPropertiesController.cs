using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Domain.Properties;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Api.Controllers;

/// <summary>Admin endpoints for property verification.</summary>
[ApiController]
[Route("api/v1/admin/properties")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminPropertiesController(IPropertyReviewService propertyReviewService) : ControllerBase
{
    /// <summary>Gets property submissions for the admin queue.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PropertyReviewResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PropertyReviewResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await propertyReviewService.GetAllAsync(cancellationToken));
    }

    /// <summary>Gets counts of property reviews by status.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<PropertyReviewSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PropertyReviewSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await propertyReviewService.GetSummaryAsync(cancellationToken));
    }

    /// <summary>Gets a property submission and its verification details.</summary>
    [HttpGet("{id:guid}", Name = "GetAdminPropertyById")]
    [ProducesResponseType<PropertyReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyReviewResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var property = await propertyReviewService.GetByIdAsync(id, cancellationToken);
        return property is null ? NotFound() : Ok(property);
    }

    /// <summary>Records the admin's verification decision.</summary>
    [HttpPatch("{id:guid}/review")]
    [ProducesResponseType<PropertyReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyReviewResponse>> Review(
        Guid id,
        ReviewPropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Status == PropertyReviewStatus.Pending)
        {
            ModelState.AddModelError(nameof(request.Status), "A review decision cannot return a submission to pending.");
            return ValidationProblem(ModelState);
        }

        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(reviewerId, out var reviewerUserId))
        {
            return Unauthorized();
        }

        var property = await propertyReviewService.ReviewAsync(id, reviewerUserId, request, cancellationToken);
        return property is null ? NotFound() : Ok(property);
    }
}
