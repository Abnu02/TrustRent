using MediatR;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.Commands;

public record ApprovePropertyCommand(
    Guid PropertyId,
    string? AuditorName = "Licensed Auditor"
) : IRequest<Result<PropertyCreatedResponse, PropertyError>>;

public class ApprovePropertyCommandHandler(ITrustRentDataStore dataStore)
    : IRequestHandler<ApprovePropertyCommand, Result<PropertyCreatedResponse, PropertyError>>
{
    public async Task<Result<PropertyCreatedResponse, PropertyError>> Handle(
        ApprovePropertyCommand command,
        CancellationToken ct)
    {
        var property = dataStore.Properties.FirstOrDefault(p => p.Id == command.PropertyId);
        if (property is null)
        {
            return Result<PropertyCreatedResponse, PropertyError>.Failure(
                PropertyError.NotFound(command.PropertyId));
        }

        property.Status = PropertyStatus.Approved;
        property.IsVerified = true;
        property.VerifiedAt = DateTime.UtcNow;
        property.VerifiedBy = command.AuditorName ?? "Licensed Auditor";
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
