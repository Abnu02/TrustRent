using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Properties;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Api.Controllers;

/// <summary>Admin property audit history.</summary>
[ApiController]
[Route("api/v1/admin/audit-log")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminAuditLogController(IPropertyReviewService propertyReviewService) : ControllerBase
{
    /// <summary>Gets recent property submissions and review decisions.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PropertyReviewEventResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PropertyReviewEventResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await propertyReviewService.GetAuditLogAsync(cancellationToken));
    }
}
