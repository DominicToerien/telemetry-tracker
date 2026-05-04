using System.Text;
using telemetry_tracker.Telemetry.Lmu.Interop;

namespace telemetry_tracker.Telemetry.Lmu;

public sealed class LmuTelemetryProvider : ITelemetryProvider
{
    private readonly object _gate = new();
    private LmuTelemetryState _state;

    public LmuTelemetryProvider()
    {
        _state = new LmuTelemetryState
        {
            Enabled = true,
            SupportedPlatform = OperatingSystem.IsWindows(),
            Connected = false,
            Message = "Waiting for LMU shared memory."
        };
    }

    public TelemetryStatusDto GetStatus()
    {
        lock (_gate)
        {
            return new TelemetryStatusDto
            {
                Provider = "lmu",
                Enabled = _state.Enabled,
                SupportedPlatform = _state.SupportedPlatform,
                Connected = _state.Connected,
                LastSuccessfulReadUtc = _state.LastSuccessfulReadUtc,
                LastScoringUpdateUtc = _state.LastScoringUpdateUtc,
                LastTelemetryUpdateUtc = _state.LastTelemetryUpdateUtc,
                LastEvent = _state.LastEvent?.ToString(),
                Message = _state.Message
            };
        }
    }

    public TelemetryDebugDto GetDebugSnapshot()
    {
        lock (_gate)
        {
            return new TelemetryDebugDto
            {
                Provider = "lmu",
                Connected = _state.Connected,
                LastSuccessfulReadUtc = _state.LastSuccessfulReadUtc,
                LastEvent = _state.LastEvent?.ToString(),
                Session = _state.ScoringInfo?.mSession,
                TrackName = TrimSdkString(_state.ScoringInfo?.mTrackName),
                PlayerName = TrimSdkString(_state.ScoringInfo?.mPlayerName),
                ActiveVehicles = _state.ActiveVehicles,
                PlayerVehicleIndex = _state.PlayerVehicleIndex,
                PlayerHasVehicle = _state.PlayerHasVehicle,
                ScoringVehicleCount = _state.Vehicles?.Length,
                Message = _state.Message
            };
        }
    }

    internal void SetEnabled(bool enabled)
    {
        lock (_gate)
        {
            _state = _state with
            {
                Enabled = enabled,
                Connected = false,
                Message = enabled ? _state.Message : "LMU telemetry reader is disabled."
            };
        }
    }

    internal void MarkUnsupportedPlatform()
    {
        lock (_gate)
        {
            _state = _state with
            {
                SupportedPlatform = false,
                Connected = false,
                Message = "LMU shared memory is only supported on Windows."
            };
        }
    }

    internal void MarkDisconnected(string message)
    {
        lock (_gate)
        {
            _state = _state with
            {
                Connected = false,
                Message = message
            };
        }
    }

    internal void ApplySharedMemorySnapshot(SharedMemoryObjectOut snapshot, DateTimeOffset capturedAtUtc)
    {
        lock (_gate)
        {
            var next = _state with
            {
                SupportedPlatform = true,
                Connected = true,
                LastSuccessfulReadUtc = capturedAtUtc,
                Generic = snapshot.generic,
                LastEvent = GetLastEvent(snapshot.generic.events),
                Message = null
            };

            if (HasEvent(snapshot.generic.events, SharedMemoryEvent.SME_UPDATE_SCORING))
            {
                var vehicleCount = Math.Clamp(snapshot.scoring.scoringInfo.mNumVehicles, 0, SharedMemoryInteropConstants.MaxVehicles);
                next = next with
                {
                    ScoringInfo = snapshot.scoring.scoringInfo,
                    Vehicles = snapshot.scoring.vehScoringInfo.Take(vehicleCount).ToArray(),
                    ResultsStream = DecodeResultsStream(snapshot.scoring.scoringStream, snapshot.scoring.scoringStreamSize),
                    LastScoringUpdateUtc = capturedAtUtc
                };
            }

            if (HasEvent(snapshot.generic.events, SharedMemoryEvent.SME_UPDATE_TELEMETRY))
            {
                var activeVehicles = Math.Clamp((int)snapshot.telemetry.activeVehicles, 0, SharedMemoryInteropConstants.MaxVehicles);
                next = next with
                {
                    ActiveVehicles = snapshot.telemetry.activeVehicles,
                    PlayerVehicleIndex = snapshot.telemetry.playerVehicleIdx,
                    PlayerHasVehicle = snapshot.telemetry.playerHasVehicle,
                    TelemetryVehicles = snapshot.telemetry.telemInfo.Take(activeVehicles).ToArray(),
                    LastTelemetryUpdateUtc = capturedAtUtc
                };
            }

            if (HasEvent(snapshot.generic.events, SharedMemoryEvent.SME_ENTER) ||
                HasEvent(snapshot.generic.events, SharedMemoryEvent.SME_EXIT) ||
                HasEvent(snapshot.generic.events, SharedMemoryEvent.SME_SET_ENVIRONMENT))
            {
                next = next with
                {
                    Paths = snapshot.paths,
                    LastPathsUpdateUtc = capturedAtUtc
                };
            }

            _state = next;
        }
    }

    private static string? DecodeResultsStream(byte[]? scoringStream, nuint scoringStreamSize)
    {
        if (scoringStream is null || scoringStream.Length == 0)
        {
            return null;
        }

        var size = (int)Math.Min((ulong)scoringStream.Length, scoringStreamSize);
        if (size <= 0)
        {
            return string.Empty;
        }

        return Encoding.ASCII.GetString(scoringStream, 0, size).TrimEnd('\0');
    }

    private static bool HasEvent(uint[]? events, SharedMemoryEvent sharedMemoryEvent)
    {
        if (events is null)
        {
            return false;
        }

        var index = (int)sharedMemoryEvent;
        return index >= 0 && index < events.Length && events[index] != 0;
    }

    private static SharedMemoryEvent? GetLastEvent(uint[]? events)
    {
        if (events is null)
        {
            return null;
        }

        SharedMemoryEvent? lastEvent = null;
        for (var i = 0; i < Math.Min(events.Length, SharedMemoryInteropConstants.EventCount); i++)
        {
            if (events[i] != 0)
            {
                lastEvent = (SharedMemoryEvent)i;
            }
        }

        return lastEvent;
    }

    private static string? TrimSdkString(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.TrimEnd('\0', ' ');
}
