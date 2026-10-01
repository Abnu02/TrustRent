using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;

namespace TrustRent.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse, PropertyError>>;

public class LoginCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<LoginCommand, Result<AuthResponse, PropertyError>>
{
    public Task<Result<AuthResponse, PropertyError>> Handle(
        LoginCommand command,
        CancellationToken ct)
    {
        var user = dataStore.Users.FirstOrDefault(u =>
            u.Email.Equals(command.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            return Task.FromResult(Result<AuthResponse, PropertyError>.Failure(
                PropertyError.ValidationError("Invalid email or credentials.")));
        }

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{user.Role}")) +
                    ".signature";

        var response = new AuthResponse(
            token,
            new UserDto(user.Id, user.FullName, user.Email, user.Role)
        );

        return Task.FromResult(Result<AuthResponse, PropertyError>.Success(response));
    }
}
