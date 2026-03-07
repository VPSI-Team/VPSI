using HwSimulator.App.Models;

namespace HwSimulator.App.Simulators;

public static class LprCameraSimulator
{
    private static readonly string[] RegionCodes = ["A", "B", "C", "E", "H", "J", "K", "L", "M", "P", "S", "T", "U", "Z"];

    public static HwEvent Generate(string deviceId, string lane, string direction, string? overridePlate = null)
    {
        var plate = overridePlate ?? GenerateCzechPlate();
        var confidence = Math.Round(Random.Shared.NextDouble() * 0.14 + 0.85, 2); // 0.85–0.99

        return new HwEvent
        {
            DeviceId = deviceId,
            EventType = "LPR_PLATE_READ",
            OccurredAt = DateTimeOffset.UtcNow,
            IdempotencyKey = $"lpr-{lane}-{Guid.NewGuid():N}",
            Payload = new LprPayload(plate, confidence, lane, direction)
        };
    }

    public static HwEvent GenerateLowConfidence(string deviceId, string lane, string direction)
    {
        var plate = GenerateCzechPlate();

        return new HwEvent
        {
            DeviceId = deviceId,
            EventType = "LPR_PLATE_READ",
            OccurredAt = DateTimeOffset.UtcNow,
            IdempotencyKey = $"lpr-{lane}-{Guid.NewGuid():N}",
            Payload = new LprPayload(plate, Math.Round(Random.Shared.NextDouble() * 0.35 + 0.40, 2), lane, direction)
        };
    }

    private static string GenerateCzechPlate()
    {
        var region = RegionCodes[Random.Shared.Next(RegionCodes.Length)];
        var digit1 = Random.Shared.Next(1, 10);
        var letter = (char)('A' + Random.Shared.Next(26));
        var digits = Random.Shared.Next(1000, 10000);
        return $"{region}{digit1}{letter}{digits}";
    }
}
