namespace Parking.Contracts.V1.Sessions;

public sealed record RegisterEntryRequest(
    string PlateNumber,
    string? CountryCode,
    Guid ParkingLotId,
    Guid? DeviceId
);
