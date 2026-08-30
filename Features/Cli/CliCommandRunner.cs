using System.Text.Json;
using telemetry_tracker.Features.TelemetryStatus;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Features.Setups;
using telemetry_tracker.Features.TelemetryData;

namespace telemetry_tracker.Features.Cli;

public sealed class CliCommandRunner(
    ITelemetryDataQueries data,
    ITelemetryStatusQueries telemetry,
    ITrackingControl tracking,
    ISetupRevisionStore setups)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task<int?> RunAsync(string[] args, TextWriter output, CancellationToken cancellationToken)
    {
        if (args.Length == 0 || !IsCliCommand(args[0]))
        {
            return null;
        }

        object? result = args[0].ToLowerInvariant() switch
        {
            "sessions" when args.ElementAtOrDefault(1) == "list" => await data.ListSessionsAsync(cancellationToken),
            "sessions" when args.ElementAtOrDefault(1) == "show" => await GetSessionAsync(args, cancellationToken),
            "laps" when args.ElementAtOrDefault(1) == "list" => await ListLapsAsync(args, cancellationToken),
            "laps" when args.ElementAtOrDefault(1) == "show" => await GetLapAsync(args, cancellationToken),
            "laps" when args.ElementAtOrDefault(1) == "compare" => await CompareLapsAsync(args, cancellationToken),
            "telemetry" when args.ElementAtOrDefault(1) == "status" => telemetry.GetStatus(),
            "telemetry" when args.ElementAtOrDefault(1) == "show" => await GetLapTelemetryAsync(args, cancellationToken),
            "tracking" when args.ElementAtOrDefault(1) == "status" => tracking.GetStatus(),
            "setup" when args.ElementAtOrDefault(1) == "list" => await ListSetupsAsync(args, cancellationToken),
            "setup" when args.ElementAtOrDefault(1) == "files" && args.ElementAtOrDefault(2) == "list" => await ListSetupFilesAsync(args, cancellationToken),
            "setup" when args.ElementAtOrDefault(1) == "import" && Guid.TryParse(GetOption(args, "--session"), out var importSessionId) => await ImportBaselineAsync(args, importSessionId, cancellationToken),
            "setup" when args.ElementAtOrDefault(1) == "show" => await ShowSetupAsync(args, cancellationToken),
            "setup" when args.ElementAtOrDefault(1) == "compare" => await CompareSetupsAsync(args, cancellationToken),
            "setup" when args.ElementAtOrDefault(1) == "propose" && Guid.TryParse(GetOption(args, "--session"), out var proposalSessionId) => await CreateProposalAsync(args, proposalSessionId, cancellationToken),
            _ => new { error = "Unknown command.", usage = "sessions list | sessions show <id> | laps list --session <id> | laps show <id> | telemetry show --lap <id> | laps compare <id> <id> | telemetry status | tracking status | setup files list --root <path> | setup import --session <id> --file <path> | setup list --session <id> | setup show <id> | setup compare <id> <id>" }
        };

        await output.WriteLineAsync(JsonSerializer.Serialize(result, JsonOptions));
        return result is { } value && value.GetType().GetProperty("error") is not null ? 1 : 0;
    }

    public static bool IsCliCommand(string value) => value.Equals("sessions", StringComparison.OrdinalIgnoreCase) ||
                                                       value.Equals("laps", StringComparison.OrdinalIgnoreCase) ||
                                                       value.Equals("telemetry", StringComparison.OrdinalIgnoreCase) ||
                                                       value.Equals("tracking", StringComparison.OrdinalIgnoreCase) ||
                                                       value.Equals("setup", StringComparison.OrdinalIgnoreCase);

    private async Task<object> CreateProposalAsync(string[] args, Guid sessionId, CancellationToken cancellationToken)
    {
        var name = GetOption(args, "--name");
        var feedback = GetOption(args, "--feedback");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(feedback)) return new { error = "--name and --feedback are required." };
        var result = await setups.CreateProposalAsync(new CreateSetupProposalCommand(sessionId, name, feedback), cancellationToken);
        return result.Error is null ? result.Proposal! : new { error = result.Error };
    }

    private async Task<object> ImportBaselineAsync(string[] args, Guid sessionId, CancellationToken cancellationToken)
    {
        var filePath = GetOption(args, "--file");
        if (string.IsNullOrWhiteSpace(filePath)) return new { error = "--file is required." };
        var result = await setups.ImportBaselineAsync(new ImportSetupBaselineCommand(sessionId, filePath), cancellationToken);
        return result.Error is null ? result.Baseline! : new { error = result.Error };
    }

    private static async Task<object> ListSetupFilesAsync(string[] args, CancellationToken cancellationToken)
    {
        var root = GetOption(args, "--root");
        if (string.IsNullOrWhiteSpace(root)) return new { error = "--root is required." };
        try
        {
            return await SvmSetupDiscovery.DiscoverAsync(root, cancellationToken);
        }
        catch (Exception exception) when (exception is ArgumentException or DirectoryNotFoundException or IOException or UnauthorizedAccessException)
        {
            return new { error = exception.Message };
        }
    }

    private async Task<object> ListSetupsAsync(string[] args, CancellationToken cancellationToken)
    {
        var sessionId = GetGuidOption(args, "--session");
        if (sessionId is null) return new { error = "--session must be a valid session ID." };
        return await setups.ListAsync(sessionId.Value, cancellationToken);
    }

    private async Task<object> ShowSetupAsync(string[] args, CancellationToken cancellationToken)
    {
        var revisionId = GetGuidArgument(args, 2);
        if (revisionId is null) return new { error = "A valid setup revision ID is required." };
        return (object?)await setups.GetAsync(revisionId.Value, cancellationToken) ?? new { error = $"Setup revision {revisionId} was not found or is not an LMU baseline." };
    }

    private async Task<object> CompareSetupsAsync(string[] args, CancellationToken cancellationToken)
    {
        var firstId = GetGuidArgument(args, 2);
        var secondId = GetGuidArgument(args, 3);
        if (firstId is null || secondId is null) return new { error = "Two valid setup revision IDs are required." };
        return (object?)await setups.CompareAsync(firstId.Value, secondId.Value, cancellationToken) ?? new { error = "Both revisions must be LMU baselines for the same exact car." };
    }

    private static string? GetOption(string[] args, string option) => args.SkipWhile(arg => arg != option).Skip(1).FirstOrDefault();
    private static Guid? GetGuidOption(string[] args, string option) => Guid.TryParse(GetOption(args, option), out var value) ? value : null;
    private static Guid? GetGuidArgument(string[] args, int index) => index < args.Length && Guid.TryParse(args[index], out var value) ? value : null;

    private async Task<object> ListLapsAsync(string[] args, CancellationToken cancellationToken)
    {
        var sessionText = GetOption(args, "--session");
        if (sessionText is not null && !Guid.TryParse(sessionText, out _)) return new { error = "--session must be a valid session ID." };
        return await data.ListLapsAsync(GetGuidOption(args, "--session"), cancellationToken);
    }

    private async Task<object> GetSessionAsync(string[] args, CancellationToken cancellationToken)
    {
        var sessionId = GetGuidArgument(args, 2);
        if (sessionId is null) return new { error = "A valid session ID is required." };
        return (object?)await data.GetSessionAsync(sessionId.Value, cancellationToken) ?? new { error = $"Session {sessionId} was not found." };
    }

    private async Task<object> GetLapAsync(string[] args, CancellationToken cancellationToken)
    {
        var lapId = GetGuidArgument(args, 2);
        if (lapId is null) return new { error = "A valid lap ID is required." };
        return (object?)await data.GetLapAsync(lapId.Value, cancellationToken) ?? new { error = $"Lap {lapId} was not found." };
    }

    private async Task<object> GetLapTelemetryAsync(string[] args, CancellationToken cancellationToken)
    {
        var lapId = GetGuidOption(args, "--lap");
        if (lapId is null) return new { error = "A valid lap ID is required via --lap." };
        return (object?)await data.GetLapTelemetryAsync(lapId.Value, cancellationToken) ?? new { error = $"Telemetry for lap {lapId} was not found." };
    }

    private async Task<object> CompareLapsAsync(string[] args, CancellationToken cancellationToken)
    {
        var firstId = GetGuidArgument(args, 2);
        var secondId = GetGuidArgument(args, 3);
        if (firstId is null || secondId is null) return new { error = "Two valid lap IDs are required." };
        return (object?)await data.CompareLapsAsync(firstId.Value, secondId.Value, cancellationToken) ?? new { error = "Both lap IDs must exist." };
    }
}
