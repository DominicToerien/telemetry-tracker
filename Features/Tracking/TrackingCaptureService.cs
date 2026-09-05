namespace telemetry_tracker.Features.Tracking;

public sealed class TrackingCaptureService : ITrackingControl
{
    private static readonly TimeSpan SampleInterval = TimeSpan.FromMilliseconds(100);
    private readonly object _gate = new();
    private readonly List<TrackingTelemetryFrame> _currentLapFrames = [];
    private Guid? _sessionId;
    private DateTimeOffset? _startedAtUtc;
    private int? _currentLapNumber;
    private bool _currentLapStartedAtBoundary;
    private DateTimeOffset? _lastSampleAtUtc;
    private int _completedLapCount;

    public TrackingStatus Start(DateTimeOffset startedAtUtc)
    {
        lock (_gate)
        {
            if (_sessionId is not null)
            {
                return CreateStatus();
            }

            _sessionId = Guid.NewGuid();
            _startedAtUtc = startedAtUtc;
            _currentLapNumber = null;
            _currentLapStartedAtBoundary = false;
            _lastSampleAtUtc = null;
            _currentLapFrames.Clear();
            _completedLapCount = 0;
            return CreateStatus();
        }
    }

    public TrackingStatus Stop()
    {
        lock (_gate)
        {
            _sessionId = null;
            _startedAtUtc = null;
            _currentLapNumber = null;
            _currentLapStartedAtBoundary = false;
            _lastSampleAtUtc = null;
            _currentLapFrames.Clear();
            return CreateStatus();
        }
    }

    public TrackingStatus GetStatus()
    {
        lock (_gate)
        {
            return CreateStatus();
        }
    }

    public CapturedLap? Observe(TrackingTelemetryFrame frame)
    {
        lock (_gate)
        {
            if (_sessionId is null)
            {
                return null;
            }

            if (_currentLapNumber is null)
            {
                _currentLapNumber = frame.LapNumber;
                return null;
            }
            else if (_currentLapNumber != frame.LapNumber)
            {
                var startedAtBoundary = frame.LapElapsedSeconds >= 0 && frame.LapElapsedSeconds <= 5;
                var completedLap = _currentLapStartedAtBoundary &&
                                   frame.LapNumber == _currentLapNumber + 1 &&
                                   startedAtBoundary
                    ? BuildCompletedLap()
                    : null;
                _currentLapFrames.Clear();
                _currentLapNumber = frame.LapNumber;
                _currentLapStartedAtBoundary = startedAtBoundary;
                _lastSampleAtUtc = null;
                if (startedAtBoundary)
                {
                    AddSample(frame);
                }

                if (completedLap is not null)
                {
                    _completedLapCount++;
                }

                return completedLap;
            }

            if (!_currentLapStartedAtBoundary)
            {
                return null;
            }

            AddSample(frame);
            return null;
        }
    }

    private void AddSample(TrackingTelemetryFrame frame)
    {
        if (_lastSampleAtUtc is not null && frame.CapturedAtUtc - _lastSampleAtUtc < SampleInterval)
        {
            return;
        }

        _currentLapFrames.Add(frame);
        _lastSampleAtUtc = frame.CapturedAtUtc;
    }

    private CapturedLap? BuildCompletedLap()
    {
        if (_currentLapFrames.Count == 0 || _sessionId is null || _currentLapNumber is null)
        {
            return null;
        }

        var frames = _currentLapFrames;
        var trace = frames.Select(frame => new LapTraceSample(
            Math.Max(0, frame.LapElapsedSeconds),
            frame.SpeedKph,
            frame.Throttle,
            frame.Brake,
            frame.Steering,
            frame.Gear,
            frame.Rpm,
            frame.PositionX,
            frame.PositionY,
            frame.PositionZ)).ToArray();

        var gearChanges = frames.Zip(frames.Skip(1), static (previous, current) => previous.Gear != current.Gear).Count(static changed => changed);
        return new CapturedLap(
            _sessionId.Value,
            _currentLapNumber.Value,
            frames[0].CapturedAtUtc,
            frames[^1].CapturedAtUtc,
            Math.Max(0, frames[^1].LapElapsedSeconds),
            frames.Average(static frame => frame.SpeedKph),
            frames.Max(static frame => frame.SpeedKph),
            frames.Min(static frame => frame.SpeedKph),
            frames.Average(static frame => frame.Throttle),
            frames.Average(static frame => frame.Brake),
            frames.Max(static frame => frame.Brake),
            frames.Average(static frame => frame.Steering),
            frames.Max(static frame => Math.Abs(frame.Steering)),
            gearChanges,
            frames.Max(static frame => frame.Gear),
            frames.Min(static frame => frame.Gear),
            trace);
    }

    private TrackingStatus CreateStatus() => new(
        _sessionId is not null,
        _sessionId,
        _startedAtUtc,
        _currentLapNumber,
        _currentLapFrames.Count,
        _completedLapCount);
}
