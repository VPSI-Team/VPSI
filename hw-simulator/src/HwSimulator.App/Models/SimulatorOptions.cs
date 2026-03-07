namespace HwSimulator.App.Models;

/// <summary>
/// Konfigurace simulátoru z appsettings / env proměnných.
/// </summary>
public sealed class SimulatorOptions
{
    public const string SectionName = "Simulator";

    public string ApiBaseUrl { get; set; } = "http://localhost:5001";
    public string Scenario { get; set; } = "NormalFlow";

    public DeviceIds Devices { get; set; } = new();
    public TimingOptions Timing { get; set; } = new();
}

public sealed class DeviceIds
{
    public string LprEntry { get; set; } = "b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a11";
    public string LprExit { get; set; } = "b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a12";
    public string BarrierEntry { get; set; } = "c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b22";
    public string BarrierExit { get; set; } = "c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b23";
    public string OccupancySensor { get; set; } = "d7f3c6c4-ae6f-4d3e-ad3c-1d6d6faf6c33";
}

public sealed class TimingOptions
{
    /// <summary>Prodleva mezi cykly scénáře v sekundách.</summary>
    public int CycleDelaySeconds { get; set; } = 10;

    /// <summary>Prodleva mezi jednotlivými eventy v rámci cyklu v ms.</summary>
    public int StepDelayMs { get; set; } = 1500;

    /// <summary>Simulovaná doba parkování v sekundách.</summary>
    public int ParkingDurationSeconds { get; set; } = 5;
}
