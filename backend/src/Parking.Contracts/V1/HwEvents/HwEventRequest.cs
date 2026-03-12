using System.Text.Json;
using Parking.Contracts.Enums;

namespace Parking.Contracts.V1.HwEvents;

public sealed record HwEventRequest(
    Guid DeviceId,
    HwEventType EventType,
    DateTimeOffset OccurredAt,
    string IdempotencyKey,
    JsonElement Payload
);
