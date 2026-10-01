using MediatR;
using TrustRent.Application.Common;

namespace TrustRent.Application.Auth.Commands;

public record VerifyLandlordCommand(Guid LandlordId) : IRequest<Result<Guid, PropertyError>>;

public class VerifyLandlordCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<VerifyLandlordCommand, Result<Guid, PropertyError>>
{
    public async Task<Result<Guid, PropertyError>> Handle(
        VerifyLandlordCommand command,
        CancellationToken ct)
    {
        var user = dataStore.Users.FirstOrDefault(u => u.Id == command.LandlordId);
        if (user is null || user.Role != "Landlord")
        {
            return Result<Guid, PropertyError>.Failure(
                PropertyError.LandlordNotFound(command.LandlordId));
        }

        user.IsVerified = true;
        await dataStore.SaveChangesAsync(ct);
        return Result<Guid, PropertyError>.Success(user.Id);
    }
}
