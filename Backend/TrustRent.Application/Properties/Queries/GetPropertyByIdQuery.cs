using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;

namespace TrustRent.Application.Properties.Queries;

public record GetPropertyByIdQuery(Guid PropertyId)
    : IRequest<Result<PropertyDetailResponse, PropertyError>>;

public class GetPropertyByIdQueryHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<GetPropertyByIdQuery, Result<PropertyDetailResponse, PropertyError>>
{
    public Task<Result<PropertyDetailResponse, PropertyError>> Handle(
        GetPropertyByIdQuery query,
        CancellationToken ct)
    {
        var property = dataStore.Properties.FirstOrDefault(p => p.Id == query.PropertyId);
        if (property is null)
        {
            return Task.FromResult(Result<PropertyDetailResponse, PropertyError>.Failure(
                PropertyError.NotFound(query.PropertyId)));
        }

        var landlord = dataStore.Users.FirstOrDefault(u => u.Id == property.LandlordId);
        var landlordContact = new LandlordContactDto(
            landlord?.Id ?? property.LandlordId,
            landlord?.FullName ?? "Abreham Bekele",
            landlord?.PhoneNumber ?? "0912345678",
            landlord?.Email ?? "abreham@example.com",
            landlord?.IsVerified ?? true
        );

        var response = new PropertyDetailResponse(
            property.Id,
            property.Title,
            property.Description,
            property.PropertyType,
            property.Rent,
            property.Deposit,
            property.Location,
            property.Bedrooms,
            property.Bathrooms,
            property.IsVerified,
            landlordContact
        );

        return Task.FromResult(Result<PropertyDetailResponse, PropertyError>.Success(response));
    }
}
