using MediatR;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;
using TrustRent.Application.Common;

namespace TrustRent.Application.Properties.Queries;

public record GetLandlordStatsQuery(Guid LandlordId) : IRequest<LandlordStatsDto>;

public class GetLandlordStatsQueryHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<GetLandlordStatsQuery, LandlordStatsDto>
{
    public Task<LandlordStatsDto> Handle(
        GetLandlordStatsQuery query,
        CancellationToken ct)
    {
        var properties = dataStore.Properties
            .Where(p => p.LandlordId == query.LandlordId)
            .ToList();

        var total = properties.Count;
        var pending = properties.Count(p => p.Status == PropertyStatus.Pending);
        var approved = properties.Count(p => p.Status == PropertyStatus.Approved);
        var rejected = properties.Count(p => p.Status == PropertyStatus.Rejected);
        var totalRevenue = properties
            .Where(p => p.Status == PropertyStatus.Approved)
            .Sum(p => p.Rent);

        var stats = new LandlordStatsDto(
            total,
            pending,
            approved,
            rejected,
            totalRevenue
        );

        return Task.FromResult(stats);
    }
}
