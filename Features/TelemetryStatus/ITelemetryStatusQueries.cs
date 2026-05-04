namespace telemetry_tracker.Features.TelemetryStatus;

public interface ITelemetryStatusQueries
{
    TelemetryStatusResponse GetStatus();
    TelemetryDebugResponse GetDebugSnapshot();
}
