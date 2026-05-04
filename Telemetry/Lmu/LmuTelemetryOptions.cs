namespace telemetry_tracker.Telemetry.Lmu;

public sealed class LmuTelemetryOptions
{
    public const string SectionName = "LmuTelemetry";

    public bool Enabled { get; set; } = true;
    public int RetryIntervalSeconds { get; set; } = 5;
    public bool DebugLogging { get; set; }
    public bool AutoEnablePluginOnStartup { get; set; } = true;
    public string? GameInstallPath { get; set; }
    public string? CustomPluginVariablesPath { get; set; }
    public string[] PluginDllNames { get; set; } =
    [
        "rFactor2SharedMemoryMapPlugin64.dll",
        "rF2SharedMemoryMapPlugin64.dll",
        "rF2SharedMemeryMapPlugin.dll"
    ];

    internal TimeSpan RetryInterval =>
        TimeSpan.FromSeconds(RetryIntervalSeconds <= 0 ? 5 : RetryIntervalSeconds);
}
