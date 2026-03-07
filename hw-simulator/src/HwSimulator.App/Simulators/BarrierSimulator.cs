using HwSimulator.App.Models;

namespace HwSimulator.App.Simulators;

public static class BarrierSimulator
{
    public static HwEvent Generate(string deviceId, string state, string triggeredBy)
    {
        return new HwEvent
        {
            DeviceId = deviceId,
            EventType = "BARRIER_STATE_CHANGED",
            OccurredAt = DateTimeOffset.UtcNow,
            IdempotencyKey = $"barrier-{deviceId[..8]}-{Guid.NewGuid():N}",
            Payload = new BarrierPayload(state, triggeredBy)
        };
    }
}
