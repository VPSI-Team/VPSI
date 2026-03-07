using HwSimulator.App.Models;
using HwSimulator.App.Simulators;
using HwSimulator.App.Transport;

namespace HwSimulator.App.Scenarios;

/// <summary>
/// Testovací scénář: duplicitní idempotencyKey, nízká confidence, burst eventů.
/// </summary>
public sealed class ErrorScenario(
    IEventSender sender,
    SimulatorOptions options,
    ILogger logger) : IScenario
{
    public string Name => "ErrorScenario";

    public async Task RunAsync(CancellationToken ct)
    {
        var devices = options.Devices;
        var timing = options.Timing;
        var cycle = 0;

        while (!ct.IsCancellationRequested)
        {
            cycle++;
            logger.LogInformation("=== ErrorScenario cyklus #{Cycle} ===", cycle);

            // 1) Duplicitní idempotencyKey — stejný event odeslaný 2×
            logger.LogWarning("Test: duplicitní idempotencyKey");
            var lprEvent = LprCameraSimulator.Generate(devices.LprEntry, "ENTRY_1", "IN");
            await sender.SendAsync(lprEvent, ct);
            await Task.Delay(300, ct);
            await sender.SendAsync(lprEvent, ct); // Duplicita
            await Task.Delay(timing.StepDelayMs, ct);

            // 2) Nízká confidence LPR
            logger.LogWarning("Test: nízká confidence LPR (< 0.75)");
            var lowConf = LprCameraSimulator.GenerateLowConfidence(devices.LprEntry, "ENTRY_1", "IN");
            await sender.SendAsync(lowConf, ct);
            await Task.Delay(timing.StepDelayMs, ct);

            // 3) Burst eventů — 5 eventů rychle za sebou
            logger.LogWarning("Test: burst 5 eventů");
            var tasks = Enumerable.Range(0, 5).Select(i =>
            {
                var evt = OccupancySensorSimulator.Generate(
                    devices.OccupancySensor, i % 2 == 0);
                return sender.SendAsync(evt, ct);
            });
            await Task.WhenAll(tasks);

            logger.LogInformation("=== ErrorScenario cyklus #{Cycle} dokončen ===", cycle);
            await Task.Delay(TimeSpan.FromSeconds(timing.CycleDelaySeconds), ct);
        }
    }
}
