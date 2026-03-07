using HwSimulator.App.Models;
using HwSimulator.App.Transport;

namespace HwSimulator.App.Scenarios;

public static class ScenarioFactory
{
    public static IScenario Create(
        string name,
        IEventSender sender,
        SimulatorOptions options,
        ILoggerFactory loggerFactory)
    {
        return name switch
        {
            "NormalFlow" => new NormalFlowScenario(sender, options, loggerFactory.CreateLogger<NormalFlowScenario>()),
            "ErrorScenario" => new ErrorScenario(sender, options, loggerFactory.CreateLogger<ErrorScenario>()),
            _ => throw new ArgumentException($"Neznámý scénář: '{name}'. Podporované: NormalFlow, ErrorScenario.")
        };
    }
}
