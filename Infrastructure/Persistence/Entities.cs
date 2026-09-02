namespace telemetry_tracker.Infrastructure.Persistence;

public sealed class SessionRecord
{
    public Guid Id { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public string? TrackName { get; set; }
    public string? VehicleName { get; set; }
    public string? ConditionsJson { get; set; }
    public List<LapSummaryRecord> Laps { get; set; } = [];
}

public sealed class LapSummaryRecord
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public SessionRecord? Session { get; set; }
    public int LapNumber { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset CompletedAtUtc { get; set; }
    public double LapTimeSeconds { get; set; }
    public double AverageSpeedKph { get; set; }
    public double MaxSpeedKph { get; set; }
    public double MinSpeedKph { get; set; }
    public double AverageThrottle { get; set; }
    public double AverageBrake { get; set; }
    public double MaxBrake { get; set; }
    public double AverageSteering { get; set; }
    public double MaxSteering { get; set; }
    public int GearChanges { get; set; }
    public int TopGear { get; set; }
    public int LowestGear { get; set; }
    public int SampleCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public LapTraceRecord? Trace { get; set; }
}

public sealed class LapTraceRecord
{
    public Guid Id { get; set; }
    public Guid LapSummaryId { get; set; }
    public LapSummaryRecord? LapSummary { get; set; }
    public int SampleRateHz { get; set; }
    public int TraceFormatVersion { get; set; }
    public required string SamplesJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class SetupRevisionRecord
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? SourceLapId { get; set; }
    public Guid? ParentRevisionId { get; set; }
    public required string Name { get; set; }
    public string? CarIdentifier { get; set; }
    public string? SetupFormat { get; set; }
    public required string SetupValuesJson { get; set; }
    public string? Rationale { get; set; }
    public string? Tradeoffs { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
