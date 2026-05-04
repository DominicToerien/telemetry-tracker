namespace telemetry_tracker.Telemetry.Lmu;

internal sealed record LmuConsoleTelemetrySnapshot
{
    public required bool Connected { get; init; }
    public string? Message { get; init; }
    public int? LapNumber { get; init; }
    public double? SpeedKph { get; init; }
    public double? Throttle { get; init; }
    public double? Brake { get; init; }
    public double? Steering { get; init; }
    public int? Gear { get; init; }
    public double? Rpm { get; init; }
    public double? FuelLiters { get; init; }
    public double? MaxBrakePressure { get; init; }
}
