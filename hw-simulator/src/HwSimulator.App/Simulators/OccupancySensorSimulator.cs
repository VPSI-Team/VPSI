using HwSimulator.App.Models;

namespace HwSimulator.App.Simulators;

public static class OccupancySensorSimulator
{
    public static HwEvent Generate(string deviceId, bool occupied, string? spotCode = null)
    {
        var spot = spotCode ?? $"A-{Random.Shared.Next(1, 51):D3}";

        return new HwEvent
        {
            DeviceId = deviceId,
            EventType = "SPOT_OCCUPANCY_CHANGED",
            OccurredAt = DateTimeOffset.UtcNow,
            IdempotencyKey = $"spot-{spot}-{Guid.NewGuid():N}",
            Payload = new SpotPayload(spot, occupied)
        };
    }
}
