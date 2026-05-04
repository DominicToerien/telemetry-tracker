namespace telemetry_tracker.Features.TelemetryStatus;

public sealed class GetTelemetryDebugHandler
{
    private readonly ITelemetryStatusQueries _queries;

    public GetTelemetryDebugHandler(ITelemetryStatusQueries queries)
    {
        _queries = queries;
    }

    public TelemetryDebugResponse Handle(GetTelemetryDebugQuery query) => _queries.GetDebugSnapshot();
}
