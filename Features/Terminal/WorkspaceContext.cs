namespace telemetry_tracker.Features.Terminal;

public sealed class WorkspaceContext
{
    public Guid? SessionId { get; private set; }
    public Guid? LapId { get; private set; }

    public void OpenSession(Guid sessionId)
    {
        SessionId = sessionId;
        LapId = null;
    }

    public void OpenLap(Guid lapId) => LapId = lapId;
    public void Back() => LapId = null;
    public string Prompt => SessionId is null ? "telemetry-tracker /" : LapId is null ? $"telemetry-tracker / {SessionId}" : $"telemetry-tracker / {SessionId} / {LapId}";
}
