namespace telemetry_tracker.Features.Tracking;

public sealed record StartTrackingCommand;
public sealed record StopTrackingCommand;
public sealed record GetTrackingStatusQuery;

public sealed class StartTrackingHandler(ITrackingControl tracking)
{
    public TrackingStatus Handle(StartTrackingCommand command) => tracking.Start(DateTimeOffset.UtcNow);
}

public sealed class StopTrackingHandler(ITrackingControl tracking)
{
    public TrackingStatus Handle(StopTrackingCommand command) => tracking.Stop();
}

public sealed class GetTrackingStatusHandler(ITrackingControl tracking)
{
    public TrackingStatus Handle(GetTrackingStatusQuery query) => tracking.GetStatus();
}
