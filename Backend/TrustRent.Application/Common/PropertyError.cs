namespace TrustRent.Application.Common;

public sealed record PropertyError(string Code, string Message)
{
    public static PropertyError NotFound(Guid id) =>
        new("property_not_found", $"Property with ID '{id}' was not found.");

    public static PropertyError LandlordNotFound(Guid id) =>
        new("landlord_not_found", $"Landlord with ID '{id}' was not found.");

    public static PropertyError UnauthorizedLandlord() =>
        new("unauthorized", "You are not authorized to perform operations on this property.");

    public static PropertyError InvalidStatusTransition(string message) =>
        new("invalid_status_transition", message);

    public static PropertyError ValidationError(string message) =>
        new("validation_error", message);

    public static PropertyError Conflict(string message) =>
        new("conflict", message);
}
