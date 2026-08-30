using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.TelemetryStatus;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Infrastructure.Persistence;
using telemetry_tracker.Features.Setups;

namespace telemetry_tracker.Features.Cli;

public sealed class CliCommandRunner(
    IDbContextFactory<TelemetryTrackerDbContext> dbContextFactory,
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
            "sessions" when args.ElementAtOrDefault(1) == "list" => await ListSessionsAsync(cancellationToken),
            "laps" when args.ElementAtOrDefault(1) == "list" => await ListLapsAsync(GetOption(args, "--session"), cancellationToken),
            "laps" when args.ElementAtOrDefault(1) == "show" => await ShowLapAsync(GetRequiredGuid(args, 2), cancellationToken),
            "laps" when args.ElementAtOrDefault(1) == "compare" => await CompareLapsAsync(GetRequiredGuid(args, 2), GetRequiredGuid(args, 3), cancellationToken),
            "telemetry" when args.ElementAtOrDefault(1) == "status" => telemetry.GetStatus(),
            "tracking" when args.ElementAtOrDefault(1) == "status" => tracking.GetStatus(),
            "setup" when args.ElementAtOrDefault(1) == "list" && Guid.TryParse(GetOption(args, "--session"), out var sessionId) => await setups.ListAsync(sessionId, cancellationToken),
            "setup" when args.ElementAtOrDefault(1) == "propose" && Guid.TryParse(GetOption(args, "--session"), out var proposalSessionId) => await CreateProposalAsync(args, proposalSessionId, cancellationToken),
            _ => new { error = "Unknown command.", usage = "sessions list | laps list --session <id> | laps show <id> | laps compare <id> <id> | telemetry status | tracking status" }
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
        var proposal = await setups.CreateProposalAsync(new CreateSetupProposalCommand(sessionId, name, feedback), cancellationToken);
        return proposal is null ? new { error = "Session was not found." } : proposal;
    }

    private async Task<object> ListSessionsAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await db.Sessions
            .Select(session => new { session.Id, session.StartedAtUtc, session.EndedAtUtc, session.TrackName, session.VehicleName, LapCount = session.Laps.Count })
            .ToListAsync(cancellationToken);
        return sessions.OrderByDescending(session => session.StartedAtUtc).ToList();
    }

    private async Task<object> ListLapsAsync(string? sessionIdText, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = db.LapSummaries.AsNoTracking();
        if (Guid.TryParse(sessionIdText, out var sessionId)) query = query.Where(lap => lap.SessionId == sessionId);
        var laps = await query
            .Select(lap => new { lap.Id, lap.SessionId, lap.LapNumber, lap.LapTimeSeconds, lap.AverageSpeedKph, lap.MaxSpeedKph, lap.SampleCount, lap.CompletedAtUtc })
            .ToListAsync(cancellationToken);
        return laps.OrderByDescending(lap => lap.CompletedAtUtc).ToList();
    }

    private async Task<object> ShowLapAsync(Guid lapId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lap = await db.LapSummaries.AsNoTracking().Include(item => item.Trace).SingleOrDefaultAsync(item => item.Id == lapId, cancellationToken);
        return lap is null ? new { error = $"Lap {lapId} was not found." } : lap;
    }

    private async Task<object> CompareLapsAsync(Guid firstId, Guid secondId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var laps = await db.LapSummaries.AsNoTracking().Where(lap => lap.Id == firstId || lap.Id == secondId).ToListAsync(cancellationToken);
        if (laps.Count != 2) return new { error = "Both lap IDs must exist." };
        var first = laps.Single(lap => lap.Id == firstId);
        var second = laps.Single(lap => lap.Id == secondId);
        return new { firstId, secondId, lapTimeDeltaSeconds = second.LapTimeSeconds - first.LapTimeSeconds, averageSpeedDeltaKph = second.AverageSpeedKph - first.AverageSpeedKph, maxSpeedDeltaKph = second.MaxSpeedKph - first.MaxSpeedKph };
    }

    private static string? GetOption(string[] args, string option) => args.SkipWhile(arg => arg != option).Skip(1).FirstOrDefault();
    private static Guid GetRequiredGuid(string[] args, int index) => index < args.Length && Guid.TryParse(args[index], out var value) ? value : throw new ArgumentException("A valid lap ID is required.");
}
