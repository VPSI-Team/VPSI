namespace Parking.Contracts.V1.HwEvents;

public sealed record HwEventResponse(
    string Status,
    string CorrelationId,
    string Processing
);
