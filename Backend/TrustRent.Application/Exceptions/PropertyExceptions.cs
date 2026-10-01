namespace TrustRent.Application.Exceptions;

public class PropertyNotFoundException : Exception
{
    public Guid PropertyId { get; }

    public PropertyNotFoundException(Guid propertyId)
        : base($"Property with ID '{propertyId}' was not found in the system.")
    {
        PropertyId = propertyId;
    }
}

public class PropertyValidationException : ArgumentException
{
    public string FieldName { get; }

    public PropertyValidationException(string fieldName, string message)
        : base(message, fieldName)
    {
        FieldName = fieldName;
    }
}

public class UnauthorizedPropertyAccessException : UnauthorizedAccessException
{
    public Guid PropertyId { get; }
    public Guid LandlordId { get; }

    public UnauthorizedPropertyAccessException(Guid propertyId, Guid landlordId)
        : base($"Landlord '{landlordId}' is not authorized to modify property '{propertyId}'.")
    {
        PropertyId = propertyId;
        LandlordId = landlordId;
    }
}
