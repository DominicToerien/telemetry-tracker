using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.Cli;
using telemetry_tracker.Features.Setups;
using telemetry_tracker.Features.TelemetryData;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Infrastructure.Persistence;
using telemetry_tracker.Telemetry.Lmu;

namespace telemetry_tracker.Tests;

public sealed class CliCommandRunnerTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"telemetry-tracker-cli-{Guid.NewGuid():N}.db");
    private readonly DbContextOptions<TelemetryTrackerDbContext> _options;
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _lapId = Guid.NewGuid();

    public CliCommandRunnerTests()
    {
        _options = new DbContextOptionsBuilder<TelemetryTrackerDbContext>().UseSqlite($"Data Source={_databasePath}").Options;
        using var db = new TelemetryTrackerDbContext(_options);
        db.Database.EnsureCreated();
        db.Sessions.Add(new SessionRecord { Id = _sessionId, StartedAtUtc = new DateTimeOffset(2026, 8, 30, 9, 0, 0, TimeSpan.Zero), TrackName = "Spa", VehicleName = "Porsche" });
        db.LapSummaries.Add(new LapSummaryRecord
        {
            Id = _lapId,
            SessionId = _sessionId,
            LapNumber = 4,
            StartedAtUtc = new DateTimeOffset(2026, 8, 30, 9, 0, 0, TimeSpan.Zero),
            CompletedAtUtc = new DateTimeOffset(2026, 8, 30, 9, 2, 0, TimeSpan.Zero),
            LapTimeSeconds = 120,
            AverageSpeedKph = 180,
            MaxSpeedKph = 280,
            MinSpeedKph = 60,
            AverageThrottle = .7,
            AverageBrake = .2,
            MaxBrake = .9,
            AverageSteering = .1,
            MaxSteering = .4,
            GearChanges = 12,
            TopGear = 7,
            LowestGear = 2,
            SampleCount = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Trace = new LapTraceRecord { Id = Guid.NewGuid(), SampleRateHz = 10, TraceFormatVersion = 1, SamplesJson = "[{\"T\":0,\"Speed\":100,\"Throttle\":0.5,\"Brake\":0,\"Steering\":0,\"Gear\":3,\"Rpm\":7000,\"X\":1,\"Y\":2,\"Z\":3}]", CreatedAtUtc = DateTimeOffset.UtcNow }
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task RecordedData_IsExposedInSessionToTelemetryHierarchy()
    {
        var runner = CreateRunner();

        var session = await RunAsync(runner, "sessions", "show", _sessionId.ToString(), "--json");
        var lap = await RunAsync(runner, "laps", "show", _lapId.ToString(), "--json");
        var telemetry = await RunAsync(runner, "telemetry", "show", "--lap", _lapId.ToString(), "--json");

        Assert.Equal(_sessionId.ToString(), session.RootElement.GetProperty("id").GetString());
        Assert.Equal(_lapId.ToString(), session.RootElement.GetProperty("laps")[0].GetProperty("id").GetString());
        Assert.False(lap.RootElement.TryGetProperty("trace", out _));
        Assert.Equal(_lapId.ToString(), telemetry.RootElement.GetProperty("lapId").GetString());
        Assert.Equal(100, telemetry.RootElement.GetProperty("samples")[0].GetProperty("speed").GetDouble());
    }

    [Fact]
    public async Task TelemetryShow_RequiresALapIdentifier()
    {
        var result = await RunAsync(CreateRunner(), "telemetry", "show", "--json");

        Assert.Contains("valid lap ID", result.RootElement.GetProperty("error").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SetupProposal_RefusesToCreateAnEmptyLmuSetup()
    {
        var result = await RunAsync(CreateRunner(), "setup", "propose", "--session", _sessionId.ToString(), "--name", "more rotation", "--feedback", "understeer on entry", "--json");

        Assert.Contains("validated, car-specific baseline", result.RootElement.GetProperty("error").GetString(), StringComparison.Ordinal);
        await using var db = new TelemetryTrackerDbContext(_options);
        Assert.Empty(await db.SetupRevisions.ToListAsync());
    }

    [Fact]
    public async Task LapsList_RejectsAnInvalidSessionFilter()
    {
        var result = await RunAsync(CreateRunner(), "laps", "list", "--session", "not-a-guid", "--json");

        Assert.Contains("valid session ID", result.RootElement.GetProperty("error").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SetupImport_RejectsAnInvalidSessionIdentifier()
    {
        var result = await RunAsync(CreateRunner(), "setup", "import", "--session", "not-a-guid", "--file", "baseline.svm", "--json");

        Assert.Contains("valid session ID", result.RootElement.GetProperty("error").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SetupModify_CreatesVersionedBmwProposalFromLapAndFeedback()
    {
        var fixturePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "references", "lmu-setups", "bmw-m4-lmgt3", "monza-mid-df.svm"));
        var store = new SetupRevisionStore(new TestDbContextFactory(_options));
        var imported = await store.ImportBaselineAsync(new ImportSetupBaselineCommand(_sessionId, fixturePath), CancellationToken.None);

        var result = await RunAsync(
            CreateRunner(),
            "setup", "modify",
            "--source", imported.Baseline!.Id.ToString(),
            "--lap", _lapId.ToString(),
            "--name", "higher-downforce",
            "--feedback", "rear instability",
            "--set", "RWSetting=11",
            "--set", "RearAntiSwaySetting=0",
            "--json");

        Assert.Equal("proposal", result.RootElement.GetProperty("proposal").GetProperty("status").GetString());
        Assert.Equal(2, result.RootElement.GetProperty("changes").GetArrayLength());
        await using var db = new TelemetryTrackerDbContext(_options);
        var proposal = await db.SetupRevisions.SingleAsync(item => item.Status == "proposal");
        Assert.Equal(imported.Baseline.Id, proposal.ParentRevisionId);
        Assert.Equal(_lapId, proposal.SourceLapId);
        Assert.Contains("rear instability", proposal.Rationale, StringComparison.Ordinal);
        var stored = JsonSerializer.Deserialize<StoredSvmSetup>(proposal.SetupValuesJson);
        Assert.Contains("RWSetting=11//4.6 deg", System.Text.Encoding.Latin1.GetString(Convert.FromBase64String(stored!.RawContentBase64)));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath)) File.Delete(_databasePath);
    }

    private CliCommandRunner CreateRunner()
    {
        var factory = new TestDbContextFactory(_options);
        var provider = new LmuTelemetryProvider();
        return new CliCommandRunner(new TelemetryDataQueries(factory), provider, new TrackingCaptureService(), new SetupRevisionStore(factory));
    }

    private static async Task<JsonDocument> RunAsync(CliCommandRunner runner, params string[] args)
    {
        using var output = new StringWriter();
        await runner.RunAsync(args, output, CancellationToken.None);
        return JsonDocument.Parse(output.ToString());
    }

    private sealed class TestDbContextFactory(DbContextOptions<TelemetryTrackerDbContext> options) : IDbContextFactory<TelemetryTrackerDbContext>
    {
        public TelemetryTrackerDbContext CreateDbContext() => new(options);
        public Task<TelemetryTrackerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}
