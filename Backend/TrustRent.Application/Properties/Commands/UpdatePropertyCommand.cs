using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;

namespace TrustRent.Application.Properties.Commands;

public record UpdatePropertyCommand(
    Guid PropertyId,
    Guid LandlordId,
    string Title,
    string Description,
    string PropertyType,
    decimal Rent,
    decimal Deposit,
    string Location,
    int Bedrooms,
    int Bathrooms
) : IRequest<Result<PropertyDetailResponse, PropertyError>>;

public class UpdatePropertyCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<UpdatePropertyCommand, Result<PropertyDetailResponse, PropertyError>>
{
    public async Task<Result<PropertyDetailResponse, PropertyError>> Handle(
        UpdatePropertyCommand command,
        CancellationToken ct)
    {
        var property = dataStore.Properties.FirstOrDefault(p => p.Id == command.PropertyId);
        if (property is null)
        {
            return Result<PropertyDetailResponse, PropertyError>.Failure(
                PropertyError.NotFound(command.PropertyId));
        }

        if (property.LandlordId != command.LandlordId)
        {
            return Result<PropertyDetailResponse, PropertyError>.Failure(
                PropertyError.UnauthorizedLandlord());
        }

        if (string.IsNullOrWhiteSpace(command.Title))
            return Result<PropertyDetailResponse, PropertyError>.Failure(PropertyError.ValidationError("Title is required."));
        if (string.IsNullOrWhiteSpace(command.Description))
            return Result<PropertyDetailResponse, PropertyError>.Failure(PropertyError.ValidationError("Description is required."));
        if (string.IsNullOrWhiteSpace(command.Location))
            return Result<PropertyDetailResponse, PropertyError>.Failure(PropertyError.ValidationError("Location is required."));
        if (command.Rent <= 0)
            return Result<PropertyDetailResponse, PropertyError>.Failure(PropertyError.ValidationError("Rent must be greater than 0 ETB."));
        if (command.Deposit < 0)
            return Result<PropertyDetailResponse, PropertyError>.Failure(PropertyError.ValidationError("Deposit cannot be negative."));

        property.Title = command.Title.Trim();
        property.Description = command.Description.Trim();
        property.PropertyType = string.IsNullOrWhiteSpace(command.PropertyType) ? property.PropertyType : command.PropertyType.Trim();
        property.Rent = command.Rent;
        property.Deposit = command.Deposit;
        property.Location = command.Location.Trim();
        property.Bedrooms = command.Bedrooms;
        property.Bathrooms = command.Bathrooms;

        await dataStore.SaveChangesAsync(ct);

        var landlord = dataStore.Users.FirstOrDefault(u => u.Id == property.LandlordId);
        var landlordContact = new LandlordContactDto(
            landlord?.Id ?? property.LandlordId,
            landlord?.FullName ?? "Abreham Bekele",
            landlord?.PhoneNumber ?? "0912345678",
            landlord?.Email ?? "abreham@example.com",
            landlord?.IsVerified ?? true
        );

        return Result<PropertyDetailResponse, PropertyError>.Success(
            new PropertyDetailResponse(
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
            )
        );
    }
}
