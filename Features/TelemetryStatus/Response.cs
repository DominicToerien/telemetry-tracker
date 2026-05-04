namespace telemetry_tracker.Features.TelemetryStatus;

public sealed class TelemetryStatusResponse
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

public sealed class TelemetryDebugResponse
{
    public required string Provider { get; init; }
    public bool Connected { get; init; }
    public DateTimeOffset? LastSuccessfulReadUtc { get; init; }
    public string? LastEvent { get; init; }
    public int? Session { get; init; }
    public string? TrackName { get; init; }
    public string? PlayerName { get; init; }
    public int? ActiveVehicles { get; init; }
    public int? PlayerVehicleIndex { get; init; }
    public bool? PlayerHasVehicle { get; init; }
    public int? ScoringVehicleCount { get; init; }
    public string? Message { get; init; }
}
