using Microsoft.Extensions.Options;
using telemetry_tracker.Telemetry.Lmu.Native;

namespace telemetry_tracker.Telemetry.Lmu;

public sealed class LmuTelemetryBackgroundService : BackgroundService
{
    private static readonly TimeSpan EventWaitTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ConsoleOutputInterval = TimeSpan.FromSeconds(1);

    private readonly LmuTelemetryProvider _provider;
    private readonly ILogger<LmuTelemetryBackgroundService> _logger;
    private readonly LmuTelemetryOptions _options;

    public LmuTelemetryBackgroundService(
        LmuTelemetryProvider provider,
        IOptions<LmuTelemetryOptions> options,
        ILogger<LmuTelemetryBackgroundService> logger)
    {
        _provider = provider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _provider.SetEnabled(_options.Enabled);

        if (!_options.Enabled)
        {
            _logger.LogInformation("LMU telemetry reader is disabled by configuration.");
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            _provider.MarkUnsupportedPlatform();
            _logger.LogWarning("LMU shared memory is unavailable because the process is not running on Windows.");
            return;
        }

        var staleAfter = TimeSpan.FromSeconds(Math.Max(_options.RetryInterval.TotalSeconds * 2, 10));
        var lastWarning = string.Empty;
        var loggedConnected = false;
        var packetsSinceConsoleOutput = 0;
        var lastConsoleOutputUtc = DateTimeOffset.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var session = LmuSharedMemorySession.Open();
                if (!loggedConnected)
                {
                    _logger.LogInformation("Connected to LMU shared memory.");
                    loggedConnected = true;
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (session.WaitForUpdate((uint)EventWaitTimeout.TotalMilliseconds))
                    {
                        var snapshot = session.CopySnapshot();
                        _provider.ApplySharedMemorySnapshot(snapshot, DateTimeOffset.UtcNow);
                        packetsSinceConsoleOutput++;
                        lastWarning = string.Empty;

                        if (_options.DebugLogging)
                        {
                            _logger.LogDebug("Processed LMU shared-memory update.");
                        }
                    }

                    if (DateTimeOffset.UtcNow - lastConsoleOutputUtc >= ConsoleOutputInterval)
                    {
                        WriteTelemetryConsoleLine(packetsSinceConsoleOutput);
                        packetsSinceConsoleOutput = 0;
                        lastConsoleOutputUtc = DateTimeOffset.UtcNow;
                    }

                    var status = _provider.GetStatus();
                    if (status.LastSuccessfulReadUtc is { } lastRead &&
                        DateTimeOffset.UtcNow - lastRead > staleAfter)
                    {
                        const string staleMessage = "LMU shared memory stopped producing updates.";
                        _provider.MarkDisconnected(staleMessage);
                        _logger.LogWarning(staleMessage);
                        loggedConnected = false;
                        break;
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                var message = $"LMU shared memory unavailable: {ex.Message}";
                _provider.MarkDisconnected(message);

                if (!string.Equals(lastWarning, message, StringComparison.Ordinal))
                {
                    _logger.LogWarning(ex, "Unable to read LMU shared memory. Retrying in {RetryInterval}.", _options.RetryInterval);
                    lastWarning = message;
                }

                loggedConnected = false;
                await Task.Delay(_options.RetryInterval, stoppingToken);
            }
        }
    }

    private void WriteTelemetryConsoleLine(int packetsPerSecond)
    {
        var snapshot = _provider.GetConsoleSnapshot();
        const string trackingState = "false";

        if (!snapshot.Connected)
        {
            var message = string.IsNullOrWhiteSpace(snapshot.Message)
                ? "LMU shared memory unavailable"
                : snapshot.Message;

            _logger.LogInformation(
                "[Telemetry] connected=false | tracking={Tracking} | packets/sec={PacketsPerSecond} | message=\"{Message}\"",
                trackingState,
                packetsPerSecond,
                message);
            return;
        }

        if (snapshot.LapNumber is null)
        {
            _logger.LogInformation(
                "[Telemetry] connected=true | tracking={Tracking} | packets/sec={PacketsPerSecond} | message=\"{Message}\"",
                trackingState,
                packetsPerSecond,
                snapshot.Message ?? "Connected, waiting for player telemetry.");
            return;
        }

        _logger.LogInformation(
            "[Telemetry] connected=true | tracking={Tracking} | packets/sec={PacketsPerSecond} | lap={Lap} | speed={SpeedKph} | throttle={ThrottlePercent}% | brake={BrakePercent}% | steering={Steering} | gear={Gear} | rpm={Rpm} | fuel={Fuel}L | maxBrakePressure={MaxBrakePressure}",
            trackingState,
            packetsPerSecond,
            snapshot.LapNumber,
            Math.Round(snapshot.SpeedKph ?? 0.0, 1),
            Math.Round((snapshot.Throttle ?? 0.0) * 100.0, 0),
            Math.Round((snapshot.Brake ?? 0.0) * 100.0, 0),
            Math.Round(snapshot.Steering ?? 0.0, 3),
            snapshot.Gear,
            Math.Round(snapshot.Rpm ?? 0.0, 0),
            Math.Round(snapshot.FuelLiters ?? 0.0, 2),
            Math.Round(snapshot.MaxBrakePressure ?? 0.0, 3));
    }
}
