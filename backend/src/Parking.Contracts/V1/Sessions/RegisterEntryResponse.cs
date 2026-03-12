namespace Parking.Contracts.V1.Sessions;

public sealed record RegisterEntryResponse(
    Guid SessionId,
    string PlateNumber,
    DateTimeOffset EntryAt
);
