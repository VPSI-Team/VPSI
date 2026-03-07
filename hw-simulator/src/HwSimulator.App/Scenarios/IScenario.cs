namespace HwSimulator.App.Scenarios;

public interface IScenario
{
    string Name { get; }
    Task RunAsync(CancellationToken ct);
}
