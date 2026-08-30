using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.Setups;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Tests;

public sealed class SvmSetupBaselineTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"telemetry-tracker-svm-{Guid.NewGuid():N}");
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"telemetry-tracker-setups-{Guid.NewGuid():N}.db");

    [Fact]
    public void Parse_RecognizesCarAndSettings_WithoutChangingSourceText()
    {
        var source = "VehicleClassSetting=\"Corvette_Z06_LMGT3R ELMS2025 GT3\"\r\n//VEH=example\r\n\r\n[REARWING]\r\nRWSetting=1//1.5 deg\r\n[CONTROLS]\r\nBrakePressureSetting=80//120 kgf (100%)\r\n";

        var document = SvmSetupDocument.Parse(source);

        Assert.Equal("Corvette_Z06_LMGT3R ELMS2025 GT3", document.VehicleClassSetting);
        Assert.Equal(source, document.WriteUnchanged());
        Assert.Equal("1", document.Settings.Single(setting => setting.Name == "RWSetting").Value);
        Assert.Equal("1.5 deg", document.Settings.Single(setting => setting.Name == "RWSetting").Comment);
        Assert.Equal("REARWING", document.Settings.Single(setting => setting.Name == "RWSetting").Section);
    }

    [Fact]
    public async Task DiscoverAsync_ReturnsTrackAndCarForSvmFiles()
    {
        var setupPath = await WriteSetupAsync("Daytonarc", "corvette.svm");

        var candidates = await SvmSetupDiscovery.DiscoverAsync(_temporaryDirectory, CancellationToken.None);

        var candidate = Assert.Single(candidates);
        Assert.Equal(setupPath, candidate.FilePath);
        Assert.Equal("Daytonarc", candidate.TrackName);
        Assert.Equal("Corvette_Z06_LMGT3R ELMS2025 GT3", candidate.VehicleClassSetting);
        Assert.NotEmpty(candidate.FingerprintSha256);
    }

    [Fact]
    public async Task ImportBaselineAsync_IsIdempotentAndVersionsChangedSourceForSameCar()
    {
        var setupPath = await WriteSetupAsync("Daytonarc", "corvette.svm");
        var options = new DbContextOptionsBuilder<TelemetryTrackerDbContext>().UseSqlite($"Data Source={_databasePath}").Options;
        var sessionId = Guid.NewGuid();
        await using (var db = new TelemetryTrackerDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Sessions.Add(new SessionRecord { Id = sessionId, StartedAtUtc = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }

        var store = new SetupRevisionStore(new TestDbContextFactory(options));
        var first = await store.ImportBaselineAsync(new ImportSetupBaselineCommand(sessionId, setupPath), CancellationToken.None);
        var duplicate = await store.ImportBaselineAsync(new ImportSetupBaselineCommand(sessionId, setupPath), CancellationToken.None);
        await File.WriteAllTextAsync(setupPath, "VehicleClassSetting=\"Corvette_Z06_LMGT3R ELMS2025 GT3\"\n[REARWING]\nRWSetting=2//3.0 deg\n");
        var second = await store.ImportBaselineAsync(new ImportSetupBaselineCommand(sessionId, setupPath), CancellationToken.None);

        Assert.Null(first.Error);
        Assert.Null(second.Error);
        Assert.NotNull(first.Baseline);
        Assert.NotNull(duplicate.Baseline);
        Assert.NotNull(second.Baseline);
        Assert.Equal(first.Baseline.Id, duplicate.Baseline.Id);
        Assert.Equal(first.Baseline.Id, second.Baseline.ParentRevisionId);
        var stored = JsonSerializer.Deserialize<StoredSvmSetup>(second.Baseline.SetupValuesJson);
        Assert.NotNull(stored);
        Assert.Equal(await File.ReadAllTextAsync(setupPath), stored.RawText);

        var comparison = await store.CompareAsync(first.Baseline.Id, second.Baseline.Id, CancellationToken.None);
        var difference = Assert.Single(comparison!.Differences);
        Assert.Equal("RWSetting", difference.Name);
        Assert.Equal("1", difference.FirstValue);
        Assert.Equal("2", difference.SecondValue);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_temporaryDirectory)) Directory.Delete(_temporaryDirectory, true);
        if (File.Exists(_databasePath)) File.Delete(_databasePath);
    }

    private async Task<string> WriteSetupAsync(string track, string fileName)
    {
        var directory = Path.Combine(_temporaryDirectory, track);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        await File.WriteAllTextAsync(path, "VehicleClassSetting=\"Corvette_Z06_LMGT3R ELMS2025 GT3\"\n[REARWING]\nRWSetting=1//1.5 deg\n");
        return path;
    }

    private sealed class TestDbContextFactory(DbContextOptions<TelemetryTrackerDbContext> options) : IDbContextFactory<TelemetryTrackerDbContext>
    {
        public TelemetryTrackerDbContext CreateDbContext() => new(options);
        public Task<TelemetryTrackerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}
