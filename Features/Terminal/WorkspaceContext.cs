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

    public void OpenLap(Guid lapId, Guid sessionId)
    {
        SessionId = sessionId;
        LapId = lapId;
    }

    public void Back()
    {
        if (LapId is not null)
        {
            LapId = null;
            return;
        }

        SessionId = null;
    }
    public string Prompt => SessionId is null ? "telemetry-tracker /" : LapId is null ? $"telemetry-tracker / {SessionId}" : $"telemetry-tracker / {SessionId} / {LapId}";
}
