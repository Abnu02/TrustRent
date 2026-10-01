namespace TrustRent.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string Role // "Tenant" or "Landlord"
);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role
);

public record AuthResponse(
    string AccessToken,
    UserDto User
);
