namespace Parking.Contracts.V1.Sessions;

public sealed record SessionQuoteResponse(
    Guid SessionId,
    string PlateNumber,
    DateTimeOffset EntryAt,
    DateTimeOffset QuotedAt,
    decimal Amount,
    string Currency
);
