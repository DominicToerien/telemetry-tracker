namespace telemetry_tracker.Telemetry.Lmu;

public sealed class LmuTelemetryOptions
{
    public const string SectionName = "LmuTelemetry";

    public bool Enabled { get; set; } = true;
    public int RetryIntervalSeconds { get; set; } = 5;
    public bool DebugLogging { get; set; }

    internal TimeSpan RetryInterval =>
        TimeSpan.FromSeconds(RetryIntervalSeconds <= 0 ? 5 : RetryIntervalSeconds);
}
