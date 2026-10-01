using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Api.Controllers;

/// <summary>Property listing submission endpoints.</summary>
[ApiController]
[Route("api/v1/properties")]
[Authorize(Roles = Roles.Landlord)]
public sealed class PropertiesController(IPropertyReviewService propertyReviewService) : ControllerBase
{
    /// <summary>Submits a property listing for admin verification.</summary>
    [HttpPost]
    [ProducesResponseType<PropertyReviewResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PropertyReviewResponse>> Create(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PhotoUrls.Any(uri =>
            !uri.IsAbsoluteUri || uri.Scheme is not ("http" or "https")))
        {
            ModelState.AddModelError(nameof(request.PhotoUrls), "Photo URLs must use HTTP or HTTPS.");
            return ValidationProblem(ModelState);
        }

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(ownerId, out var ownerUserId))
        {
            return Unauthorized();
        }

        var property = await propertyReviewService.CreateAsync(ownerUserId, request, cancellationToken);
        return CreatedAtRoute("GetAdminPropertyById", new { id = property.Id }, property);
    }
}
