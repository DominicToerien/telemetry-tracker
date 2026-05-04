using Microsoft.Extensions.Options;
using telemetry_tracker.Telemetry.Lmu.Native;

namespace telemetry_tracker.Telemetry.Lmu;

public sealed class LmuTelemetryBackgroundService : BackgroundService
{
    private static readonly TimeSpan EventWaitTimeout = TimeSpan.FromSeconds(1);

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
                        lastWarning = string.Empty;

                        if (_options.DebugLogging)
                        {
                            _logger.LogDebug("Processed LMU shared-memory update.");
                        }

                        continue;
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
}
