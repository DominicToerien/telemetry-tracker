namespace telemetry_tracker.Features.TelemetryStatus;

public sealed class GetTelemetryStatusHandler
{
    private readonly ITelemetryStatusQueries _queries;

    public GetTelemetryStatusHandler(ITelemetryStatusQueries queries)
    {
        _queries = queries;
    }

    public TelemetryStatusResponse Handle(GetTelemetryStatusQuery query) => _queries.GetStatus();
}
