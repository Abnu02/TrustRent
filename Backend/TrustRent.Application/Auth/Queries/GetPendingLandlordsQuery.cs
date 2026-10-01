using MediatR;
using TrustRent.Application.Common;
using TrustRent.Domain.Entities;

namespace TrustRent.Application.Auth.Queries;

public record GetPendingLandlordsQuery() : IRequest<IEnumerable<User>>;

public class GetPendingLandlordsQueryHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<GetPendingLandlordsQuery, IEnumerable<User>>
{
    public Task<IEnumerable<User>> Handle(
        GetPendingLandlordsQuery request,
        CancellationToken ct)
    {
        var pending = dataStore.Users
            .Where(u => u.Role == "Landlord" && !u.IsVerified)
            .OrderByDescending(u => u.CreatedAt)
            .ToList();

        return Task.FromResult<IEnumerable<User>>(pending);
    }
}
