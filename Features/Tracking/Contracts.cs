namespace telemetry_tracker.Features.Tracking;

public sealed record TrackingTelemetryFrame(
    DateTimeOffset CapturedAtUtc,
    int LapNumber,
    double LapElapsedSeconds,
    double SpeedKph,
    double Throttle,
    double Brake,
    double Steering,
    int Gear,
    double Rpm,
    double PositionX,
    double PositionY,
    double PositionZ);

public sealed record TrackingStatus(
    bool IsActive,
    Guid? SessionId,
    DateTimeOffset? StartedAtUtc,
    int? CurrentLapNumber,
    int BufferedSampleCount,
    int CompletedLapCount);

public sealed record LapTraceSample(
    double T,
    double Speed,
    double Throttle,
    double Brake,
    double Steering,
    int Gear,
    double Rpm,
    double X,
    double Y,
    double Z);

public sealed record CapturedLap(
    Guid SessionId,
    int LapNumber,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    double LapTimeSeconds,
    double AverageSpeedKph,
    double MaxSpeedKph,
    double MinSpeedKph,
    double AverageThrottle,
    double AverageBrake,
    double MaxBrake,
    double AverageSteering,
    double MaxSteering,
    int GearChanges,
    int TopGear,
    int LowestGear,
    IReadOnlyList<LapTraceSample> Trace);

public interface ITrackingControl
{
    TrackingStatus Start(DateTimeOffset startedAtUtc);
    TrackingStatus Stop();
    TrackingStatus GetStatus();
    CapturedLap? Observe(TrackingTelemetryFrame frame);
}
