using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/test")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { Message = "Public access" });
    }

    [HttpGet("protected")]
    [Authorize]
    public IActionResult ProtectedEndpoint()
    {
        return Ok(new { Message = "Protected access for any authenticated user", User = User.Identity?.Name });
    }

    [HttpGet("landlord-only")]
    [Authorize(Roles = Roles.Landlord)]
    public IActionResult LandlordOnlyEndpoint()
    {
        return Ok(new { Message = "Protected access for Landlords only" });
    }

    [HttpGet("tenant-only")]
    [Authorize(Roles = Roles.Tenant)]
    public IActionResult TenantOnlyEndpoint()
    {
        return Ok(new { Message = "Protected access for Tenants only" });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok(new { Message = "Protected access for Admins only" });
    }
}
