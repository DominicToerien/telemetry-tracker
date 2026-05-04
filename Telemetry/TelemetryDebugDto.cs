namespace telemetry_tracker.Telemetry;

public sealed class TelemetryDebugDto
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
