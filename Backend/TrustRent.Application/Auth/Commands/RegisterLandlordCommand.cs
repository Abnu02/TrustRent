using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Entities;

namespace TrustRent.Application.Auth.Commands;

public record RegisterLandlordCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password
) : IRequest<Result<UserDto, PropertyError>>;

public class RegisterLandlordCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<RegisterLandlordCommand, Result<UserDto, PropertyError>>
{
    public async Task<Result<UserDto, PropertyError>> Handle(
        RegisterLandlordCommand command,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.FullName))
            return Result<UserDto, PropertyError>.Failure(PropertyError.ValidationError("Full name is required."));
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result<UserDto, PropertyError>.Failure(PropertyError.ValidationError("Email is required."));
        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
            return Result<UserDto, PropertyError>.Failure(PropertyError.ValidationError("Phone number is required."));

        var existing = dataStore.Users.FirstOrDefault(u =>
            u.Email.Equals(command.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return Result<UserDto, PropertyError>.Failure(
                PropertyError.Conflict($"A user with email '{command.Email}' already exists."));
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = command.FullName.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            PhoneNumber = command.PhoneNumber.Trim(),
            Role = "Landlord",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        dataStore.Users.Add(newUser);
        await dataStore.SaveChangesAsync(ct);

        return Result<UserDto, PropertyError>.Success(
            new UserDto(newUser.Id, newUser.FullName, newUser.Email, newUser.Role)
        );
    }
}
