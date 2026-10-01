using MediatR;
using TrustRent.Application.Common;

namespace TrustRent.Application.Auth.Commands;

public record RejectLandlordCommand(Guid LandlordId) : IRequest<Result<Guid, PropertyError>>;

public class RejectLandlordCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<RejectLandlordCommand, Result<Guid, PropertyError>>
{
    public async Task<Result<Guid, PropertyError>> Handle(
        RejectLandlordCommand command,
        CancellationToken ct)
    {
        var user = dataStore.Users.FirstOrDefault(u => u.Id == command.LandlordId);
        if (user is null || user.Role != "Landlord")
        {
            return Result<Guid, PropertyError>.Failure(
                PropertyError.LandlordNotFound(command.LandlordId));
        }

        user.IsVerified = false;
        await dataStore.SaveChangesAsync(ct);
        return Result<Guid, PropertyError>.Success(user.Id);
    }
}
