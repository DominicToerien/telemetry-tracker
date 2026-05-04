namespace telemetry_tracker.Telemetry;

public interface ITelemetryProvider
{
    TelemetryStatusDto GetStatus();
    TelemetryDebugDto GetDebugSnapshot();
}
