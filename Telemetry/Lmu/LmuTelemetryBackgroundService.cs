using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Features.Laps;
using telemetry_tracker.Telemetry.Lmu.Native;

namespace telemetry_tracker.Telemetry.Lmu;

public sealed class LmuTelemetryBackgroundService : BackgroundService
{
    private static readonly TimeSpan EventWaitTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ConsoleOutputInterval = TimeSpan.FromSeconds(1);

    private readonly LmuTelemetryProvider _provider;
    private readonly ILogger<LmuTelemetryBackgroundService> _logger;
    private readonly LmuTelemetryOptions _options;
    private readonly ITrackingControl _tracking;
    private readonly ILocalLapStore _lapStore;
    private int _lastConsoleBlockHeight;
    private bool _telemetryConsoleInitialized;

    public LmuTelemetryBackgroundService(
        LmuTelemetryProvider provider,
        ITrackingControl tracking,
        ILocalLapStore lapStore,
        IOptions<LmuTelemetryOptions> options,
        ILogger<LmuTelemetryBackgroundService> logger)
    {
        _provider = provider;
        _tracking = tracking;
        _lapStore = lapStore;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield immediately so host startup can complete before telemetry acquisition begins.
        await Task.Yield();

        _provider.SetEnabled(_options.Enabled);
        ValidateLmuPrerequisites();

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
                        var completedLap = _provider.GetTrackingFrame() is { } frame
                            ? _tracking.Observe(frame)
                            : null;
                        if (completedLap is not null)
                        {
                            await _lapStore.SaveAsync(completedLap, stoppingToken);
                            _logger.LogInformation(
                                "[Lap Saved] lap={LapNumber} | time={LapTimeSeconds:F3}s | avgSpeed={AverageSpeedKph:F1} | maxSpeed={MaxSpeedKph:F1} | samples={SampleCount}",
                                completedLap.LapNumber,
                                completedLap.LapTimeSeconds,
                                completedLap.AverageSpeedKph,
                                completedLap.MaxSpeedKph,
                                completedLap.Trace.Count);
                        }
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
        var trackingState = _tracking.GetStatus().IsActive ? "true" : "false";

        if (!snapshot.Connected)
        {
            var message = string.IsNullOrWhiteSpace(snapshot.Message)
                ? "LMU shared memory unavailable"
                : snapshot.Message;

            RenderTelemetryConsoleBlock(
                [
                    "[Telemetry]",
                    "connected=false",
                    $"tracking={trackingState}",
                    $"packets/sec={packetsPerSecond}",
                    $"message=\"{message}\""
                ]);
            return;
        }

        if (snapshot.LapNumber is null)
        {
            RenderTelemetryConsoleBlock(
                [
                    "[Telemetry]",
                    "connected=true",
                    $"tracking={trackingState}",
                    $"packets/sec={packetsPerSecond}",
                    $"inRealtime={snapshot.InRealtime}",
                    $"activeVehicles={snapshot.ActiveVehicles}",
                    $"playerHasVehicle={snapshot.PlayerHasVehicle}",
                    $"playerVehicleIndex={snapshot.PlayerVehicleIndex}",
                    $"message=\"{snapshot.Message ?? "Connected, waiting for player telemetry."}\""
                ]);
            return;
        }

        RenderTelemetryConsoleBlock(
            [
                "[Telemetry]",
                "connected=true",
                $"tracking={trackingState}",
                $"packets/sec={packetsPerSecond}",
                $"lap={snapshot.LapNumber}",
                $"speed={Math.Round(snapshot.SpeedKph ?? 0.0, 1)}",
                $"throttle={Math.Round((snapshot.Throttle ?? 0.0) * 100.0, 0)}%",
                $"brake={Math.Round((snapshot.Brake ?? 0.0) * 100.0, 0)}%",
                $"steering={Math.Round(snapshot.Steering ?? 0.0, 3)}",
                $"gear={snapshot.Gear}",
                $"rpm={Math.Round(snapshot.Rpm ?? 0.0, 0)}",
                $"fuel={Math.Round(snapshot.FuelLiters ?? 0.0, 2)}L",
                $"maxBrakePressure={Math.Round(snapshot.MaxBrakePressure ?? 0.0, 3)}"
            ]);
    }

    private void RenderTelemetryConsoleBlock(IReadOnlyList<string> segments)
    {
        if (Console.IsOutputRedirected)
        {
            Console.WriteLine(string.Join(" | ", segments));
            return;
        }

        var windowWidth = Math.Max(Console.WindowWidth - 1, 20);
        var lines = WrapTelemetrySegments(segments, windowWidth);
        var totalLines = Math.Max(lines.Count, _lastConsoleBlockHeight);

        try
        {
            if (_telemetryConsoleInitialized)
            {
                Console.Write('\r');
                if (_lastConsoleBlockHeight > 1)
                {
                    Console.Write($"\u001b[{_lastConsoleBlockHeight - 1}A");
                }
            }

            for (var i = 0; i < totalLines; i++)
            {
                var line = i < lines.Count
                    ? lines[i].PadRight(windowWidth)
                    : new string(' ', windowWidth);

                Console.Write("\u001b[2K");
                Console.Write(line);

                if (i < totalLines - 1)
                {
                    Console.Write(Environment.NewLine);
                }
            }

            _telemetryConsoleInitialized = true;
            _lastConsoleBlockHeight = lines.Count;
        }
        catch (IOException)
        {
            Console.WriteLine(string.Join(" | ", segments));
            _lastConsoleBlockHeight = lines.Count;
        }
    }

    private static List<string> WrapTelemetrySegments(IReadOnlyList<string> segments, int windowWidth)
    {
        var lines = new List<string>();
        var currentLine = string.Empty;

        foreach (var segment in segments)
        {
            var candidate = string.IsNullOrEmpty(currentLine)
                ? segment
                : $"{currentLine} | {segment}";

            if (candidate.Length <= windowWidth)
            {
                currentLine = candidate;
                continue;
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = segment.Length <= windowWidth
                    ? segment
                    : segment[..windowWidth];
                continue;
            }

            lines.Add(segment[..windowWidth]);
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        if (lines.Count == 0)
        {
            lines.Add(string.Empty);
        }

        return lines;
    }

    private void ValidateLmuPrerequisites()
    {
        var gameInstallPath = ResolveGameInstallPath();
        _logger.LogInformation("LMU prerequisite check: using game install path {GameInstallPath}.", gameInstallPath);
        var pluginDirectories = new[]
        {
            Path.Combine(gameInstallPath, "Plugins"),
            Path.Combine(gameInstallPath, "Bin64", "Plugins")
        };
        var configuredDllNames = _options.PluginDllNames ?? [];

        var foundDllPath = pluginDirectories
            .SelectMany(directory => configuredDllNames.Select(fileName => Path.Combine(directory, fileName)))
            .FirstOrDefault(File.Exists);

        if (foundDllPath is not null)
        {
            _logger.LogInformation("LMU prerequisite check: found shared-memory plugin DLL at {PluginPath}.", foundDllPath);
        }
        else
        {
            _logger.LogWarning(
                "LMU prerequisite check: no expected shared-memory plugin DLL found in any expected plugin directory ({PluginDirectories}). Expected one of: {PluginNames}",
                string.Join(", ", pluginDirectories),
                string.Join(", ", configuredDllNames));
        }

        var customPluginVariablesPath = ResolveCustomPluginVariablesPath(gameInstallPath);
        _logger.LogInformation("LMU prerequisite check: using CustomPluginVariables path {CustomPluginVariablesPath}.", customPluginVariablesPath);
        if (!File.Exists(customPluginVariablesPath))
        {
            _logger.LogWarning("LMU prerequisite check: CustomPluginVariables file not found at {Path}.", customPluginVariablesPath);
            return;
        }

        ValidateAndOptionallyEnablePlugin(customPluginVariablesPath, configuredDllNames);
    }

    private string ResolveGameInstallPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.GameInstallPath))
        {
            return _options.GameInstallPath;
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var candidates = new[]
        {
            Path.Combine("C:\\Steam", "steamapps", "common", "Le Mans Ultimate"),
            Path.Combine(programFilesX86, "Steam", "steamapps", "common", "Le Mans Ultimate")
        };

        var detectedPath = candidates.FirstOrDefault(Directory.Exists);
        return detectedPath ?? candidates[0];
    }

    private string ResolveCustomPluginVariablesPath(string gameInstallPath)
    {
        if (!string.IsNullOrWhiteSpace(_options.CustomPluginVariablesPath))
        {
            return _options.CustomPluginVariablesPath;
        }

        return Path.Combine(gameInstallPath, "UserData", "player", "CustomPluginVariables.JSON");
    }

    private void ValidateAndOptionallyEnablePlugin(string customPluginVariablesPath, string[] configuredDllNames)
    {
        try
        {
            var json = File.ReadAllText(customPluginVariablesPath);
            var rootNode = JsonNode.Parse(json) as JsonObject;
            if (rootNode is null)
            {
                _logger.LogWarning("LMU prerequisite check: {Path} did not contain a top-level JSON object.", customPluginVariablesPath);
                return;
            }

            var pluginKeys = rootNode
                .Select(kvp => kvp.Key)
                .Where(key =>
                    configuredDllNames.Contains(key, StringComparer.OrdinalIgnoreCase) ||
                    (key.IndexOf("shared", StringComparison.OrdinalIgnoreCase) >= 0 &&
                     key.IndexOf("memory", StringComparison.OrdinalIgnoreCase) >= 0))
                .ToArray();

            if (pluginKeys.Length == 0)
            {
                _logger.LogWarning(
                    "LMU prerequisite check: shared-memory plugin entry not found in {Path}. Expected a key like one of: {PluginNames}",
                    customPluginVariablesPath,
                    string.Join(", ", configuredDllNames));
                return;
            }

            var changed = false;
            foreach (var pluginKey in pluginKeys)
            {
                if (rootNode[pluginKey] is not JsonObject pluginNode)
                {
                    _logger.LogWarning("LMU prerequisite check: plugin entry {PluginKey} is not a JSON object in {Path}.", pluginKey, customPluginVariablesPath);
                    continue;
                }

                var enabledPropertyName = pluginNode
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault(key => string.Equals(key.Trim(), "Enabled", StringComparison.OrdinalIgnoreCase));

                var currentState = TryReadEnabled(pluginNode, enabledPropertyName);
                if (currentState == true)
                {
                    if (_options.AutoEnablePluginOnStartup)
                    {
                        changed |= NormalizePluginSubscriptions(pluginNode);
                    }

                    _logger.LogInformation("LMU prerequisite check: shared-memory plugin {PluginKey} appears enabled in {Path}.", pluginKey, customPluginVariablesPath);
                    continue;
                }

                if (!_options.AutoEnablePluginOnStartup)
                {
                    if (currentState == false)
                    {
                        _logger.LogWarning("LMU prerequisite check: shared-memory plugin {PluginKey} appears disabled in {Path}.", pluginKey, customPluginVariablesPath);
                    }
                    else
                    {
                        _logger.LogWarning("LMU prerequisite check: unable to determine shared-memory plugin {PluginKey} enabled state from {Path}.", pluginKey, customPluginVariablesPath);
                    }
                    continue;
                }

                var targetPropertyName = enabledPropertyName ?? "Enabled";
                pluginNode[targetPropertyName] = 1;
                changed = true;
                changed |= NormalizePluginSubscriptions(pluginNode);
                _logger.LogInformation("LMU prerequisite check: auto-enabled shared-memory plugin {PluginKey} in {Path}.", pluginKey, customPluginVariablesPath);
            }

            if (changed)
            {
                PersistPluginConfig(customPluginVariablesPath, rootNode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LMU prerequisite check: failed to parse or update {Path}.", customPluginVariablesPath);
        }
    }

    private static bool? TryReadEnabled(JsonObject pluginNode, string? enabledPropertyName)
    {
        if (enabledPropertyName is null)
        {
            return null;
        }

        var enabledNode = pluginNode[enabledPropertyName];
        if (enabledNode is null)
        {
            return null;
        }

        var raw = enabledNode.ToJsonString().Trim('"', ' ');
        if (bool.TryParse(raw, out var boolResult))
        {
            return boolResult;
        }

        if (int.TryParse(raw, out var intResult))
        {
            return intResult != 0;
        }

        return null;
    }

    private bool NormalizePluginSubscriptions(JsonObject pluginNode)
    {
        var changed = false;

        const string unsubscribedBuffersMask = "UnsubscribedBuffersMask";
        if (!TryGetPropertyCaseInsensitive(pluginNode, unsubscribedBuffersMask, out var currentPropertyName))
        {
            changed |= EnsurePluginSetting(pluginNode, "EnableDirectMemoryAccess", 1);
            return changed;
        }

        var currentRaw = pluginNode[currentPropertyName]?.ToJsonString().Trim('"', ' ');
        if (!int.TryParse(currentRaw, out var currentValue))
        {
            changed |= EnsurePluginSetting(pluginNode, "EnableDirectMemoryAccess", 1);
            return changed;
        }

        if (currentValue != 0)
        {
            pluginNode[currentPropertyName] = 0;
            _logger.LogInformation(
                "LMU prerequisite check: updated {PropertyName} from {PreviousValue} to 0 to ensure telemetry buffers are subscribed.",
                currentPropertyName,
                currentValue);
            changed = true;
        }

        changed |= EnsurePluginSetting(pluginNode, "EnableDirectMemoryAccess", 1);
        return changed;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonObject node, string propertyName, out string matchedPropertyName)
    {
        foreach (var kvp in node)
        {
            if (string.Equals(kvp.Key, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                matchedPropertyName = kvp.Key;
                return true;
            }
        }

        matchedPropertyName = string.Empty;
        return false;
    }

    private bool EnsurePluginSetting(JsonObject pluginNode, string propertyName, int requiredValue)
    {
        var matchedPropertyName = propertyName;
        if (!TryGetPropertyCaseInsensitive(pluginNode, propertyName, out var existingPropertyName))
        {
            pluginNode[propertyName] = requiredValue;
            _logger.LogInformation(
                "LMU prerequisite check: added {PropertyName}={RequiredValue}.",
                propertyName,
                requiredValue);
            return true;
        }

        matchedPropertyName = existingPropertyName;
        var currentRaw = pluginNode[matchedPropertyName]?.ToJsonString().Trim('"', ' ');
        if (!int.TryParse(currentRaw, out var currentValue) || currentValue != requiredValue)
        {
            pluginNode[matchedPropertyName] = requiredValue;
            _logger.LogInformation(
                "LMU prerequisite check: updated {PropertyName} from {PreviousValue} to {RequiredValue}.",
                matchedPropertyName,
                currentRaw ?? "(null)",
                requiredValue);
            return true;
        }

        return false;
    }

    private static void PersistPluginConfig(string path, JsonObject rootNode)
    {
        var writeOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(path, rootNode.ToJsonString(writeOptions));
    }
}
