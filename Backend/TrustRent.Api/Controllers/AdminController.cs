using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;
using TrustRent.Domain.Repositories;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Tags("Admin")]
[Produces("application/json")]
public class AdminController(
    IPropertyRepository propertyRepository,
    IUserRepository userRepository) : ControllerBase
{
    // ==========================================
    // PROPERTY VERIFICATION
    // ==========================================

    [HttpGet("properties/pending")]
    [ProducesResponseType(typeof(IEnumerable<MyPropertyResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Admin views all properties awaiting verification")]
    public async Task<IActionResult> GetPendingProperties()
    {
        var pending = await propertyRepository.GetPendingAsync();
        var response = pending.Select(p => new MyPropertyResponse(
            p.Id,
            p.Title,
            p.Rent,
            p.Deposit,
            p.Location,
            p.PropertyType,
            p.Bedrooms,
            p.Bathrooms,
            p.Status.ToString(),
            p.IsVerified,
            p.CreatedAt
        ));
        return Ok(response);
    }

    [HttpPut("properties/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin approves and verifies a property listing")]
    public async Task<IActionResult> ApproveProperty([FromRoute] Guid id)
    {
        var property = await propertyRepository.GetByIdAsync(id);
        if (property == null)
        {
            return NotFound(new { message = $"Property with ID '{id}' was not found." });
        }

        property.Status = PropertyStatus.Approved;
        property.IsVerified = true;
        property.VerifiedAt = DateTime.UtcNow;
        property.VerifiedBy = "Admin";

        await propertyRepository.UpdateAsync(property);

        return Ok(new
        {
            message = $"Property '{property.Title}' has been approved and is now publicly verified!",
            property.Id,
            Status = property.Status.ToString(),
            property.IsVerified,
            property.VerifiedAt
        });
    }

    [HttpPut("properties/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin rejects a property listing")]
    public async Task<IActionResult> RejectProperty([FromRoute] Guid id)
    {
        var property = await propertyRepository.GetByIdAsync(id);
        if (property == null)
        {
            return NotFound(new { message = $"Property with ID '{id}' was not found." });
        }

        property.Status = PropertyStatus.Rejected;
        property.IsVerified = false;

        await propertyRepository.UpdateAsync(property);

        return Ok(new
        {
            message = $"Property '{property.Title}' has been rejected.",
            property.Id,
            Status = property.Status.ToString(),
            property.IsVerified
        });
    }

    // ==========================================
    // LANDLORD VERIFICATION
    // ==========================================

    [HttpGet("landlords/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Admin views all landlords awaiting identity verification")]
    public async Task<IActionResult> GetPendingLandlords()
    {
        var pending = await userRepository.GetPendingLandlordsAsync();
        return Ok(pending.Select(u => new
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
    public async Task<IActionResult> VerifyLandlord([FromRoute] Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null || user.Role != "Landlord")
        {
            return NotFound(new { message = $"Landlord with ID '{id}' was not found." });
        }

        user.IsVerified = true;
        await userRepository.UpdateAsync(user);

        return Ok(new
        {
            message = $"Landlord '{user.FullName}' has been verified successfully.",
            user.Id,
            user.IsVerified
        });
    }

    [HttpPut("landlords/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin rejects a landlord identity verification")]
    public async Task<IActionResult> RejectLandlord([FromRoute] Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null || user.Role != "Landlord")
        {
            return NotFound(new { message = $"Landlord with ID '{id}' was not found." });
        }

        user.IsVerified = false;
        await userRepository.UpdateAsync(user);

        return Ok(new
        {
            message = $"Landlord '{user.FullName}' identity verification was rejected.",
            user.Id,
            user.IsVerified
        });
    }
}
