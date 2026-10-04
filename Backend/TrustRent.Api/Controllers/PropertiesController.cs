using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.DTOs.Properties;
using TrustRent.Application.Services;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(
        IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpPost]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> Create(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var landlordId = GetCurrentUserId();

        var result =
            await _propertyService.CreateAsync(
                landlordId,
                request,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> GetMyProperties(
        CancellationToken cancellationToken)
    {
        var landlordId = GetCurrentUserId();

        var result =
            await _propertyService.GetMyPropertiesAsync(
                landlordId,
                cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var landlordId = GetCurrentUserId();

        await _propertyService.UpdateAsync(
            landlordId,
            id,
            request,
            cancellationToken);

        return Ok(new
        {
            message = "Property updated successfully."
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var landlordId = GetCurrentUserId();

        try
        {
            await _propertyService.ArchiveAsync(
                landlordId,
                id,
                cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Property was not found."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(
        [FromQuery] string? location,
        [FromQuery] decimal? minRent,
        [FromQuery] decimal? maxRent,
        [FromQuery] string? propertyType,
        [FromQuery] int? bedrooms,
        [FromQuery] int? bathrooms,
        CancellationToken cancellationToken)
    {
        var result =
            await _propertyService.GetPublicAsync(
                location,
                minRent,
                maxRent,
                propertyType,
                bedrooms,
                bathrooms,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result =
            await _propertyService.GetDetailsAsync(
                id,
                cancellationToken);

        if (result is null)
            return NotFound(new
            {
                message = "Property was not found."
            });

        return Ok(result);
    }

    // Intentional TrustRent image extension
    [HttpPost("{id:guid}/image")]
    [Authorize(Roles = "Landlord")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(
        Guid id,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            return BadRequest(new
            {
                message = "An image file is required."
            });
        }

        var landlordId = GetCurrentUserId();

        await using var stream =
            image.OpenReadStream();

        var imageUrl =
            await _propertyService.AddImageAsync(
                landlordId,
                id,
                stream,
                image.FileName,
                image.ContentType,
                cancellationToken);

        return Ok(new
        {
            message = "Property image uploaded successfully.",
            imageUrl
        });
    }

    private Guid GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid authenticated user.");
        }

        return userId;
    }
}