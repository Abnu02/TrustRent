using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.Queries;

public record GetLandlordPropertiesQuery(
    Guid LandlordId,
    int Page = 1,
    int PageSize = 6,
    string? Status = null,
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = true
) : IRequest<PagedResponse<MyPropertyResponse>>;

public class GetLandlordPropertiesQueryHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<GetLandlordPropertiesQuery, PagedResponse<MyPropertyResponse>>
{
    public Task<PagedResponse<MyPropertyResponse>> Handle(
        GetLandlordPropertiesQuery query,
        CancellationToken ct)
    {
        var items = dataStore.Properties
            .Where(p => p.LandlordId == query.LandlordId);

        // Status Filtering
        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<PropertyStatus>(query.Status, true, out var parsedStatus))
            {
                items = items.Where(p => p.Status == parsedStatus);
            }
        }

        // Search Term Filtering (by Title, Location, PropertyType)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            items = items.Where(p =>
                p.Title.ToLowerInvariant().Contains(search) ||
                p.Location.ToLowerInvariant().Contains(search) ||
                p.PropertyType.ToLowerInvariant().Contains(search));
        }

        // Sorting (TMS Course pattern)
        items = query.SortBy?.ToLowerInvariant() switch
        {
            "rent" => query.SortDescending
                ? items.OrderByDescending(p => p.Rent)
                : items.OrderBy(p => p.Rent),
            "title" => query.SortDescending
                ? items.OrderByDescending(p => p.Title)
                : items.OrderBy(p => p.Title),
            _ => query.SortDescending
                ? items.OrderByDescending(p => p.CreatedAt)
                : items.OrderBy(p => p.CreatedAt)
        };

        var allList = items.ToList();
        var totalCount = allList.Count;

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 6 : (query.PageSize > 50 ? 50 : query.PageSize);

        var pagedItems = allList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return Task.FromResult(new PagedResponse<MyPropertyResponse>(
            pagedItems,
            totalCount,
            page,
            pageSize
        ));
    }
}
