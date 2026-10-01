using MediatR;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;
using TrustRent.Application.Common;

namespace TrustRent.Application.Properties.Queries;

public record GetPendingPropertiesQuery() : IRequest<IEnumerable<MyPropertyResponse>>;

public class GetPendingPropertiesQueryHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<GetPendingPropertiesQuery, IEnumerable<MyPropertyResponse>>
{
    public Task<IEnumerable<MyPropertyResponse>> Handle(
        GetPendingPropertiesQuery request,
        CancellationToken ct)
    {
        var pending = dataStore.Properties
            .Where(p => p.Status == PropertyStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new MyPropertyResponse(
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
            ))
            .ToList();

        return Task.FromResult<IEnumerable<MyPropertyResponse>>(pending);
    }
}
