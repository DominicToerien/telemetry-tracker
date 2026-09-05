using telemetry_tracker.Features.Cli;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Features.TelemetryData;
using System.Text;

namespace telemetry_tracker.Features.Terminal;

public sealed class TerminalWorkspace(
    CliCommandRunner cli,
    StartTrackingHandler startTracking,
    StopTrackingHandler stopTracking,
    ITelemetryDataQueries data,
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

            var args = ParseArguments(input.Trim().TrimStart('/'));
            if (args.Length == 0)
            {
                continue;
            }

            if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("/sessions list | /open-session <id> | /laps list [--session <id>] | /open-lap <id> | /back | /compare <lap-a> <lap-b> | /tracking start|stop|status | /telemetry status | /setup files list --root <path> | /setup import --session <id> --file <path> | /setup list --session <id> | /setup show <id> | /setup compare <id> <id> | /setup modify --source <id> --lap <id> --name <name> --feedback <text> --set <setting=value> | /exit");
                continue;
            }

            if (args is ["open-session", var sessionText] && Guid.TryParse(sessionText, out var sessionId))
            {
                if (await data.GetSessionAsync(sessionId, cancellationToken) is null)
                {
                    Console.WriteLine($"Session {sessionId} was not found.");
                    continue;
                }

                context.OpenSession(sessionId);
                continue;
            }

            if (args is ["open-lap", var lapText] && Guid.TryParse(lapText, out var lapId))
            {
                var lap = await data.GetLapAsync(lapId, cancellationToken);
                if (lap is null)
                {
                    Console.WriteLine($"Lap {lapId} was not found.");
                    continue;
                }

                context.OpenLap(lap.Id, lap.SessionId);
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

    internal static string[] ParseArguments(string input)
    {
        var arguments = new List<string>();
        var current = new StringBuilder();
        char? quote = null;

        foreach (var character in input)
        {
            if (quote is not null)
            {
                if (character == quote)
                {
                    quote = null;
                }
                else
                {
                    current.Append(character);
                }
                continue;
            }

            if (character is '\'' or '"')
            {
                quote = character;
            }
            else if (char.IsWhiteSpace(character))
            {
                AddArgument(arguments, current);
            }
            else
            {
                current.Append(character);
            }
        }

        AddArgument(arguments, current);
        return arguments.ToArray();
    }

    private static void AddArgument(List<string> arguments, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        arguments.Add(current.ToString());
        current.Clear();
    }
}
