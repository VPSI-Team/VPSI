using System.Text.Json.Serialization;

namespace HwSimulator.App.Models;

public sealed class HwEvent
{
    [JsonPropertyName("deviceId")]
    public required string DeviceId { get; init; }

    [JsonPropertyName("eventType")]
    public required string EventType { get; init; }

    [JsonPropertyName("occurredAt")]
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("idempotencyKey")]
    public required string IdempotencyKey { get; init; }

    [JsonPropertyName("payload")]
    public required object Payload { get; init; }
}

public sealed record LprPayload(
    [property: JsonPropertyName("plate")] string Plate,
    [property: JsonPropertyName("confidence")] double Confidence,
    [property: JsonPropertyName("lane")] string Lane,
    [property: JsonPropertyName("direction")] string Direction);

public sealed record BarrierPayload(
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("triggeredBy")] string TriggeredBy);

public sealed record SpotPayload(
    [property: JsonPropertyName("spotCode")] string SpotCode,
    [property: JsonPropertyName("occupied")] bool Occupied);
