namespace TrustRent.Application.DTOs;

public record LandlordContactDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string Email,
    bool IsVerified
);

public record CreatePropertyRequest(
    string Title,
    string Description,
    string PropertyType,
    decimal Rent,
    decimal Deposit,
    string Location,
    int Bedrooms,
    int Bathrooms
);

public record UpdatePropertyRequest(
    string Title,
    string Description,
    string PropertyType,
    decimal Rent,
    decimal Deposit,
    string Location,
    int Bedrooms,
    int Bathrooms
);

public record MyPropertyResponse(
    Guid Id,
    string Title,
    decimal Rent,
    decimal Deposit,
    string Location,
    string PropertyType,
    int Bedrooms,
    int Bathrooms,
    string Status,
    bool IsVerified,
    DateTime CreatedAt
);

public record PropertyCreatedResponse(
    Guid Id,
    string Title,
    string Status,
    bool IsVerified,
    DateTime CreatedAt
);

public record PropertyDetailResponse(
    Guid Id,
    string Title,
    string Description,
    string PropertyType,
    decimal Rent,
    decimal Deposit,
    string Location,
    int Bedrooms,
    int Bathrooms,
    bool IsVerified,
    LandlordContactDto Landlord
);

public record LandlordStatsDto(
    int TotalProperties,
    int PendingCount,
    int ApprovedCount,
    int RejectedCount,
    decimal TotalMonthlyRevenueETB
);

public record RegisterLandlordRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password
);
