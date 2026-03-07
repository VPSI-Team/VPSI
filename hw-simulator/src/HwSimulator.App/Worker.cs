using HwSimulator.App.Models;
using HwSimulator.App.Scenarios;
using HwSimulator.App.Transport;
using Microsoft.Extensions.Options;

namespace HwSimulator.App;

public class Worker(
    ILogger<Worker> logger,
    IOptions<SimulatorOptions> options,
    IEventSender sender,
    ILoggerFactory loggerFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = options.Value;
        logger.LogInformation(
            "HW Simulátor startuje — scénář: {Scenario}, API: {ApiBaseUrl}",
            config.Scenario, config.ApiBaseUrl);

        var scenario = ScenarioFactory.Create(config.Scenario, sender, config, loggerFactory);
        logger.LogInformation("Spouštím scénář: {Name}", scenario.Name);

        try
        {
            await scenario.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("HW Simulátor zastaven.");
        }
    }
}
