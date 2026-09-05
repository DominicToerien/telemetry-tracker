using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using telemetry_tracker.Features.Laps;
using telemetry_tracker.Features.Setups;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Tests;

public sealed class LocalLapStoreTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"telemetry-tracker-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task SaveAsync_CreatesSessionSummaryAndTrace_WithoutDuplicatingALap()
    {
        var options = new DbContextOptionsBuilder<TelemetryTrackerDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        await using (var setupContext = new TelemetryTrackerDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        var store = new LocalLapStore(new TestDbContextFactory(options));
        var lap = CreateLap();

        await store.SaveAsync(lap, CancellationToken.None);
        await store.SaveAsync(lap, CancellationToken.None);

        await using var dbContext = new TelemetryTrackerDbContext(options);
        var session = await dbContext.Sessions.SingleAsync();
        var summary = await dbContext.LapSummaries.Include(item => item.Trace).SingleAsync();

        Assert.Equal(lap.SessionId, session.Id);
        Assert.Equal(BmwM4SetupModifier.CarIdentifier, session.VehicleName);
        Assert.Equal(lap.LapNumber, summary.LapNumber);
        Assert.Equal(2, summary.SampleCount);
        Assert.NotNull(summary.Trace);
        Assert.Equal(10, summary.Trace.SampleRateHz);
        Assert.Contains("\"T\":0", summary.Trace.SamplesJson, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    private static CapturedLap CreateLap() => new(
        Guid.NewGuid(),
        3,
        new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 8, 29, 10, 1, 30, TimeSpan.Zero),
        90,
        150,
        250,
        80,
        0.7,
        0.1,
        0.8,
        0.05,
        0.2,
        15,
        7,
        2,
        [
            new LapTraceSample(0, 120, 0.5, 0, 0, 4, 7000, 1, 2, 3),
            new LapTraceSample(0.1, 125, 0.6, 0, 0.1, 4, 7100, 4, 5, 6)
        ],
        BmwM4SetupModifier.CarIdentifier);

    private sealed class TestDbContextFactory(DbContextOptions<TelemetryTrackerDbContext> options) : IDbContextFactory<TelemetryTrackerDbContext>
    {
        public TelemetryTrackerDbContext CreateDbContext() => new(options);

        public Task<TelemetryTrackerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
