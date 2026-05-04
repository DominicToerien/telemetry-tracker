using telemetry_tracker.Telemetry.Lmu.Interop;

namespace telemetry_tracker.Telemetry.Lmu;

internal sealed record LmuTelemetryState
{
    public required bool Enabled { get; init; }
    public required bool SupportedPlatform { get; init; }
    public required bool Connected { get; init; }
    public DateTimeOffset? LastSuccessfulReadUtc { get; init; }
    public DateTimeOffset? LastScoringUpdateUtc { get; init; }
    public DateTimeOffset? LastTelemetryUpdateUtc { get; init; }
    public DateTimeOffset? LastPathsUpdateUtc { get; init; }
    public SharedMemoryEvent? LastEvent { get; init; }
    public string? Message { get; init; }
    public ScoringInfoV01? ScoringInfo { get; init; }
    public VehicleScoringInfoV01[]? Vehicles { get; init; }
    public TelemInfoV01[]? TelemetryVehicles { get; init; }
    public SharedMemoryPathData? Paths { get; init; }
    public SharedMemoryGeneric? Generic { get; init; }
    public string? ResultsStream { get; init; }
    public byte? ActiveVehicles { get; init; }
    public byte? PlayerVehicleIndex { get; init; }
    public bool? PlayerHasVehicle { get; init; }
}
