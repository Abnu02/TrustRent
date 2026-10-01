using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.Commands;

public record CreatePropertyCommand(
    Guid LandlordId,
    string Title,
    string Description,
    string PropertyType,
    decimal Rent,
    decimal Deposit,
    string Location,
    int Bedrooms,
    int Bathrooms
) : IRequest<Result<PropertyCreatedResponse, PropertyError>>;

public class CreatePropertyCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<CreatePropertyCommand, Result<PropertyCreatedResponse, PropertyError>>
{
    public async Task<Result<PropertyCreatedResponse, PropertyError>> Handle(
        CreatePropertyCommand command,
        CancellationToken ct)
    {
        var landlord = dataStore.Users.FirstOrDefault(u => u.Id == command.LandlordId);
        if (landlord is null)
        {
            return Result<PropertyCreatedResponse, PropertyError>.Failure(
                PropertyError.LandlordNotFound(command.LandlordId));
        }

        if (string.IsNullOrWhiteSpace(command.Title))
            return Result<PropertyCreatedResponse, PropertyError>.Failure(PropertyError.ValidationError("Title is required."));
        if (string.IsNullOrWhiteSpace(command.Description))
            return Result<PropertyCreatedResponse, PropertyError>.Failure(PropertyError.ValidationError("Description is required."));
        if (string.IsNullOrWhiteSpace(command.Location))
            return Result<PropertyCreatedResponse, PropertyError>.Failure(PropertyError.ValidationError("Location is required."));
        if (command.Rent <= 0)
            return Result<PropertyCreatedResponse, PropertyError>.Failure(PropertyError.ValidationError("Rent must be greater than 0 ETB."));
        if (command.Deposit < 0)
            return Result<PropertyCreatedResponse, PropertyError>.Failure(PropertyError.ValidationError("Deposit cannot be negative."));

        var property = new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = command.LandlordId,
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            PropertyType = string.IsNullOrWhiteSpace(command.PropertyType) ? "Apartment" : command.PropertyType.Trim(),
            Rent = command.Rent,
            Deposit = command.Deposit,
            Location = command.Location.Trim(),
            Bedrooms = command.Bedrooms,
            Bathrooms = command.Bathrooms,
            Status = PropertyStatus.Pending,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        dataStore.Properties.Add(property);
        await dataStore.SaveChangesAsync(ct);

        return Result<PropertyCreatedResponse, PropertyError>.Success(
            new PropertyCreatedResponse(
                property.Id,
                property.Title,
                property.Status.ToString(),
                property.IsVerified,
                property.CreatedAt
            )
        );
    }
}
