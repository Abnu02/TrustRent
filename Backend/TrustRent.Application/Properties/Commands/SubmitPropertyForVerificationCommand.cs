using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.Commands;

public record SubmitPropertyForVerificationCommand(
    Guid PropertyId,
    Guid LandlordId
) : IRequest<Result<PropertyCreatedResponse, PropertyError>>;

public class SubmitPropertyForVerificationCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<SubmitPropertyForVerificationCommand, Result<PropertyCreatedResponse, PropertyError>>
{
    public async Task<Result<PropertyCreatedResponse, PropertyError>> Handle(
        SubmitPropertyForVerificationCommand command,
        CancellationToken ct)
    {
        var property = dataStore.Properties.FirstOrDefault(p => p.Id == command.PropertyId);
        if (property is null)
        {
            return Result<PropertyCreatedResponse, PropertyError>.Failure(
                PropertyError.NotFound(command.PropertyId));
        }

        if (property.LandlordId != command.LandlordId)
        {
            return Result<PropertyCreatedResponse, PropertyError>.Failure(
                PropertyError.UnauthorizedLandlord());
        }

        if (property.Status == PropertyStatus.Approved)
        {
            return Result<PropertyCreatedResponse, PropertyError>.Failure(
                PropertyError.InvalidStatusTransition("Property is already verified and approved."));
        }

        property.Status = PropertyStatus.Pending;
        property.RejectionReason = null;
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
