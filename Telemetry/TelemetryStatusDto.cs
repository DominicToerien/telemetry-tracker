namespace telemetry_tracker.Telemetry;

public sealed class TelemetryStatusDto
{
    public required string Provider { get; init; }
    public bool Enabled { get; init; }
    public bool SupportedPlatform { get; init; }
    public bool Connected { get; init; }
    public DateTimeOffset? LastSuccessfulReadUtc { get; init; }
    public DateTimeOffset? LastScoringUpdateUtc { get; init; }
    public DateTimeOffset? LastTelemetryUpdateUtc { get; init; }
    public string? LastEvent { get; init; }
    public string? Message { get; init; }
}
