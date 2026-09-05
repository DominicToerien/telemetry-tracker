using telemetry_tracker.Features.Terminal;

namespace telemetry_tracker.Tests;

public sealed class TerminalWorkspaceTests
{
    [Fact]
    public void ParseArguments_PreservesQuotedPaths()
    {
        var arguments = TerminalWorkspace.ParseArguments("setup files list --root \"C:\\Program Files\\Le Mans Ultimate\"");

        Assert.Equal(["setup", "files", "list", "--root", "C:\\Program Files\\Le Mans Ultimate"], arguments);
    }

    [Fact]
    public void Context_OpenLapUsesItsOwningSessionAndBackReturnsToRoot()
    {
        var context = new WorkspaceContext();
        var sessionId = Guid.NewGuid();
        var lapId = Guid.NewGuid();

        context.OpenLap(lapId, sessionId);
        Assert.Equal(sessionId, context.SessionId);
        Assert.Equal(lapId, context.LapId);

        context.Back();
        Assert.Equal(sessionId, context.SessionId);
        Assert.Null(context.LapId);

        context.Back();
        Assert.Null(context.SessionId);
    }
}
