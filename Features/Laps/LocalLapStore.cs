using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Features.Laps;

public interface ILocalLapStore
{
    Task SaveAsync(CapturedLap lap, CancellationToken cancellationToken);
}

public sealed class LocalLapStore(IDbContextFactory<TelemetryTrackerDbContext> dbContextFactory) : ILocalLapStore
{
    public async Task SaveAsync(CapturedLap lap, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var session = await dbContext.Sessions.FindAsync([lap.SessionId], cancellationToken);
        if (session is null)
        {
            session = new SessionRecord
            {
                Id = lap.SessionId,
                StartedAtUtc = lap.StartedAtUtc,
                VehicleName = lap.CarIdentifier
            };
            dbContext.Sessions.Add(session);
        }
        else if (session.VehicleName is null && lap.CarIdentifier is not null)
        {
            session.VehicleName = lap.CarIdentifier;
        }

        if (await dbContext.LapSummaries.AnyAsync(existing => existing.SessionId == lap.SessionId && existing.LapNumber == lap.LapNumber, cancellationToken))
        {
            return;
        }

        var summary = new LapSummaryRecord
        {
            Id = Guid.NewGuid(),
            SessionId = lap.SessionId,
            LapNumber = lap.LapNumber,
            StartedAtUtc = lap.StartedAtUtc,
            CompletedAtUtc = lap.CompletedAtUtc,
            LapTimeSeconds = lap.LapTimeSeconds,
            AverageSpeedKph = lap.AverageSpeedKph,
            MaxSpeedKph = lap.MaxSpeedKph,
            MinSpeedKph = lap.MinSpeedKph,
            AverageThrottle = lap.AverageThrottle,
            AverageBrake = lap.AverageBrake,
            MaxBrake = lap.MaxBrake,
            AverageSteering = lap.AverageSteering,
            MaxSteering = lap.MaxSteering,
            GearChanges = lap.GearChanges,
            TopGear = lap.TopGear,
            LowestGear = lap.LowestGear,
            SampleCount = lap.Trace.Count,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Trace = new LapTraceRecord
            {
                Id = Guid.NewGuid(),
                SampleRateHz = 10,
                TraceFormatVersion = 1,
                SamplesJson = JsonSerializer.Serialize(lap.Trace),
                CreatedAtUtc = DateTimeOffset.UtcNow
            }
        };

        dbContext.LapSummaries.Add(summary);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
