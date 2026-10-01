using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Entities;

namespace TrustRent.Application.Auth.Commands;

public record RegisterUserCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string Role
) : IRequest<Result<AuthResponse, PropertyError>>;

public class RegisterUserCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<RegisterUserCommand, Result<AuthResponse, PropertyError>>
{
    public async Task<Result<AuthResponse, PropertyError>> Handle(
        RegisterUserCommand command,
        CancellationToken ct)
    {
        var existing = dataStore.Users.FirstOrDefault(u =>
            u.Email.Equals(command.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return Result<AuthResponse, PropertyError>.Failure(
                PropertyError.Conflict("A user with this email already exists."));
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = command.FullName.Trim(),
            Email = command.Email.Trim(),
            PhoneNumber = command.PhoneNumber.Trim(),
            Role = command.Role.Trim(),
            IsVerified = command.Role == "Tenant",
            CreatedAt = DateTime.UtcNow
        };

        dataStore.Users.Add(newUser);
        await dataStore.SaveChangesAsync(ct);

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{newUser.Id}:{newUser.Role}")) +
                    ".signature";

        return Result<AuthResponse, PropertyError>.Success(
            new AuthResponse(
                token,
                new UserDto(newUser.Id, newUser.FullName, newUser.Email, newUser.Role)
            )
        );
    }
}
