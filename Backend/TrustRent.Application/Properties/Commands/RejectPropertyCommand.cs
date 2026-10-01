using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.Commands;

public record RejectPropertyCommand(
    Guid PropertyId,
    string? Reason = null
) : IRequest<Result<PropertyCreatedResponse, PropertyError>>;

public class RejectPropertyCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<RejectPropertyCommand, Result<PropertyCreatedResponse, PropertyError>>
{
    public async Task<Result<PropertyCreatedResponse, PropertyError>> Handle(
        RejectPropertyCommand command,
        CancellationToken ct)
    {
        var property = dataStore.Properties.FirstOrDefault(p => p.Id == command.PropertyId);
        if (property is null)
        {
            return Result<PropertyCreatedResponse, PropertyError>.Failure(
                PropertyError.NotFound(command.PropertyId));
        }

        property.Status = PropertyStatus.Rejected;
        property.IsVerified = false;
        property.RejectionReason = command.Reason ?? "Deed documentation does not match Addis Ababa Land Management records.";

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
