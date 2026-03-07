using HwSimulator.App.Models;
using HwSimulator.App.Simulators;
using HwSimulator.App.Transport;

namespace HwSimulator.App.Scenarios;

/// <summary>
/// Kompletní cyklus: LPR vjezd → závora OPEN → závora CLOSED → spot occupied
/// → parkování → spot free → LPR výjezd → závora výjezd OPEN → závora výjezd CLOSED.
/// Běží v nekonečné smyčce.
/// </summary>
public sealed class NormalFlowScenario(
    IEventSender sender,
    SimulatorOptions options,
    ILogger logger) : IScenario
{
    public string Name => "NormalFlow";

    public async Task RunAsync(CancellationToken ct)
    {
        var devices = options.Devices;
        var timing = options.Timing;
        var cycle = 0;

        while (!ct.IsCancellationRequested)
        {
            cycle++;
            logger.LogInformation("=== NormalFlow cyklus #{Cycle} ===", cycle);

            // 1) LPR čtení na vjezdu
            var lprEntry = LprCameraSimulator.Generate(devices.LprEntry, "ENTRY_1", "IN");
            await sender.SendAsync(lprEntry, ct);
            await Task.Delay(timing.StepDelayMs, ct);

            var plate = ((LprPayload)lprEntry.Payload).Plate;

            // 2) Závora vjezd OPEN
            await sender.SendAsync(
                BarrierSimulator.Generate(devices.BarrierEntry, "OPEN", $"LPR:{plate}"), ct);
            await Task.Delay(timing.StepDelayMs, ct);

            // 3) Závora vjezd CLOSED
            await sender.SendAsync(
                BarrierSimulator.Generate(devices.BarrierEntry, "CLOSED", "AUTO_TIMER"), ct);
            await Task.Delay(timing.StepDelayMs, ct);

            // 4) Spot obsazen
            var spotCode = $"A-{Random.Shared.Next(1, 51):D3}";
            await sender.SendAsync(
                OccupancySensorSimulator.Generate(devices.OccupancySensor, true, spotCode), ct);

            // 5) Simulace parkování
            logger.LogInformation("Parkování na místě {Spot} po dobu {Seconds}s…",
                spotCode, timing.ParkingDurationSeconds);
            await Task.Delay(TimeSpan.FromSeconds(timing.ParkingDurationSeconds), ct);

            // 6) Spot volný
            await sender.SendAsync(
                OccupancySensorSimulator.Generate(devices.OccupancySensor, false, spotCode), ct);
            await Task.Delay(timing.StepDelayMs, ct);

            // 7) LPR čtení na výjezdu
            await sender.SendAsync(
                LprCameraSimulator.Generate(devices.LprExit, "EXIT_1", "OUT", plate), ct);
            await Task.Delay(timing.StepDelayMs, ct);

            // 8) Závora výjezd OPEN
            await sender.SendAsync(
                BarrierSimulator.Generate(devices.BarrierExit, "OPEN", $"LPR:{plate}"), ct);
            await Task.Delay(timing.StepDelayMs, ct);

            // 9) Závora výjezd CLOSED
            await sender.SendAsync(
                BarrierSimulator.Generate(devices.BarrierExit, "CLOSED", "AUTO_TIMER"), ct);

            logger.LogInformation("=== NormalFlow cyklus #{Cycle} dokončen ===", cycle);

            // Pauza mezi cykly
            await Task.Delay(TimeSpan.FromSeconds(timing.CycleDelaySeconds), ct);
        }
    }
}
