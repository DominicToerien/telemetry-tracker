using telemetry_tracker.Features.Cli;
using telemetry_tracker.Features.Tracking;

namespace telemetry_tracker.Features.Terminal;

public sealed class TerminalWorkspace(
    CliCommandRunner cli,
    StartTrackingHandler startTracking,
    StopTrackingHandler stopTracking,
    WorkspaceContext context)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Telemetry Tracker local workspace. Type /help for commands.");

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write($"{context.Prompt} > ");
            var input = await Console.In.ReadLineAsync(cancellationToken);
            if (input is null || input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var args = input.Trim().TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (args.Length == 0)
            {
                continue;
            }

            if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("/sessions list | /open-session <id> | /laps list [--session <id>] | /open-lap <id> | /back | /compare <lap-a> <lap-b> | /tracking start|stop|status | /telemetry status | /setup list --session <id> | /setup propose --session <id> --name <name> --feedback <feedback> | /exit");
                continue;
            }

            if (args is ["open-session", var sessionText] && Guid.TryParse(sessionText, out var sessionId))
            {
                context.OpenSession(sessionId);
                continue;
            }

            if (args is ["open-lap", var lapText] && Guid.TryParse(lapText, out var lapId))
            {
                context.OpenLap(lapId);
                continue;
            }

            if (args is ["back"])
            {
                context.Back();
                continue;
            }

            if (args[0].Equals("compare", StringComparison.OrdinalIgnoreCase))
            {
                args = ["laps", "compare", .. args.Skip(1)];
            }

            if (args is ["laps", "list"] && context.SessionId is not null)
            {
                args = ["laps", "list", "--session", context.SessionId.Value.ToString()];
            }

            if (args is ["tracking", "start"])
            {
                Console.WriteLine($"Tracking active={startTracking.Handle(new StartTrackingCommand()).IsActive}");
                continue;
            }

            if (args is ["tracking", "stop"])
            {
                Console.WriteLine($"Tracking active={stopTracking.Handle(new StopTrackingCommand()).IsActive}");
                continue;
            }

            await cli.RunAsync(args, Console.Out, cancellationToken);
        }
    }
}
