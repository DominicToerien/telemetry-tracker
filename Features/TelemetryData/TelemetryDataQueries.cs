using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Features.TelemetryData;

public sealed record SessionListItem(
    Guid Id,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    string? TrackName,
    string? VehicleName,
    int LapCount);

public sealed record LapListItem(
    Guid Id,
    Guid SessionId,
    int LapNumber,
    double LapTimeSeconds,
    double AverageSpeedKph,
    double MaxSpeedKph,
    int SampleCount,
    DateTimeOffset CompletedAtUtc);

public sealed record SessionDetails(
    Guid Id,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    string? TrackName,
    string? VehicleName,
    IReadOnlyList<LapListItem> Laps);

public sealed record LapDetails(
    Guid Id,
    Guid SessionId,
    int LapNumber,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    double LapTimeSeconds,
    double AverageSpeedKph,
    double MaxSpeedKph,
    double MinSpeedKph,
    double AverageThrottle,
    double AverageBrake,
    double MaxBrake,
    double AverageSteering,
    double MaxSteering,
    int GearChanges,
    int TopGear,
    int LowestGear,
    int SampleCount);

public sealed record LapTelemetry(
    Guid LapId,
    Guid SessionId,
    int LapNumber,
    int SampleRateHz,
    int TraceFormatVersion,
    IReadOnlyList<LapTraceSample> Samples);

public sealed record LapComparison(
    Guid FirstId,
    Guid SecondId,
    double LapTimeDeltaSeconds,
    double AverageSpeedDeltaKph,
    double MaxSpeedDeltaKph);

public interface ITelemetryDataQueries
{
    Task<IReadOnlyList<SessionListItem>> ListSessionsAsync(CancellationToken cancellationToken);
    Task<SessionDetails?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<LapListItem>> ListLapsAsync(Guid? sessionId, CancellationToken cancellationToken);
    Task<LapDetails?> GetLapAsync(Guid lapId, CancellationToken cancellationToken);
    Task<LapTelemetry?> GetLapTelemetryAsync(Guid lapId, CancellationToken cancellationToken);
    Task<LapComparison?> CompareLapsAsync(Guid firstId, Guid secondId, CancellationToken cancellationToken);
}

public sealed class TelemetryDataQueries(IDbContextFactory<TelemetryTrackerDbContext> dbContextFactory) : ITelemetryDataQueries
{
    public async Task<IReadOnlyList<SessionListItem>> ListSessionsAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Sessions.AsNoTracking()
            .OrderByDescending(session => session.StartedAtUtc)
            .Select(session => new SessionListItem(session.Id, session.StartedAtUtc, session.EndedAtUtc, session.TrackName, session.VehicleName, session.Laps.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<SessionDetails?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var session = await db.Sessions.AsNoTracking()
            .Include(item => item.Laps)
            .SingleOrDefaultAsync(item => item.Id == sessionId, cancellationToken);

        return session is null ? null : new SessionDetails(
            session.Id,
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.TrackName,
            session.VehicleName,
            session.Laps.OrderBy(lap => lap.LapNumber).Select(ToListItem).ToList());
    }

    public async Task<IReadOnlyList<LapListItem>> ListLapsAsync(Guid? sessionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = db.LapSummaries.AsNoTracking();
        if (sessionId is not null) query = query.Where(lap => lap.SessionId == sessionId);

        var laps = await query.OrderByDescending(lap => lap.CompletedAtUtc).ToListAsync(cancellationToken);
        return laps.Select(ToListItem).ToList();
    }

    public async Task<LapDetails?> GetLapAsync(Guid lapId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lap = await db.LapSummaries.AsNoTracking().SingleOrDefaultAsync(item => item.Id == lapId, cancellationToken);
        return lap is null ? null : ToDetails(lap);
    }

    public async Task<LapTelemetry?> GetLapTelemetryAsync(Guid lapId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lap = await db.LapSummaries.AsNoTracking().Include(item => item.Trace).SingleOrDefaultAsync(item => item.Id == lapId, cancellationToken);
        if (lap?.Trace is null) return null;

        var samples = JsonSerializer.Deserialize<List<LapTraceSample>>(lap.Trace.SamplesJson) ?? [];
        return new LapTelemetry(lap.Id, lap.SessionId, lap.LapNumber, lap.Trace.SampleRateHz, lap.Trace.TraceFormatVersion, samples);
    }

    public async Task<LapComparison?> CompareLapsAsync(Guid firstId, Guid secondId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var laps = await db.LapSummaries.AsNoTracking().Where(lap => lap.Id == firstId || lap.Id == secondId).ToListAsync(cancellationToken);
        if (laps.Count != 2) return null;

        var first = laps.Single(lap => lap.Id == firstId);
        var second = laps.Single(lap => lap.Id == secondId);
        return new LapComparison(firstId, secondId, second.LapTimeSeconds - first.LapTimeSeconds, second.AverageSpeedKph - first.AverageSpeedKph, second.MaxSpeedKph - first.MaxSpeedKph);
    }

    private static LapListItem ToListItem(LapSummaryRecord lap) => new(lap.Id, lap.SessionId, lap.LapNumber, lap.LapTimeSeconds, lap.AverageSpeedKph, lap.MaxSpeedKph, lap.SampleCount, lap.CompletedAtUtc);

    private static LapDetails ToDetails(LapSummaryRecord lap) => new(lap.Id, lap.SessionId, lap.LapNumber, lap.StartedAtUtc, lap.CompletedAtUtc, lap.LapTimeSeconds, lap.AverageSpeedKph, lap.MaxSpeedKph, lap.MinSpeedKph, lap.AverageThrottle, lap.AverageBrake, lap.MaxBrake, lap.AverageSteering, lap.MaxSteering, lap.GearChanges, lap.TopGear, lap.LowestGear, lap.SampleCount);
}
