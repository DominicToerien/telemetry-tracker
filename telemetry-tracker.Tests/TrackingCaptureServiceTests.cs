using telemetry_tracker.Features.Tracking;

namespace telemetry_tracker.Tests;

public sealed class TrackingCaptureServiceTests
{
    [Fact]
    public void Start_IsIdempotentUntilStopped()
    {
        var service = new TrackingCaptureService();
        var startedAt = new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero);

        var first = service.Start(startedAt);
        var second = service.Start(startedAt.AddMinutes(1));

        Assert.True(first.IsActive);
        Assert.Equal(first.SessionId, second.SessionId);
        Assert.Equal(startedAt, second.StartedAtUtc);

        var stopped = service.Stop();

        Assert.False(stopped.IsActive);
        Assert.Null(stopped.SessionId);
        Assert.Equal(0, stopped.BufferedSampleCount);
    }

    [Fact]
    public void Observe_CompletesPreviousLapAndDownsamplesTrace()
    {
        var service = new TrackingCaptureService();
        var start = new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero);
        service.Start(start);

        Assert.Null(service.Observe(Frame(start, 4, 45.00, 180, 0.8, 0.0, 0.2, 5)));
        Assert.Null(service.Observe(Frame(start.AddMilliseconds(50), 4, 45.05, 185, 0.9, 0.0, 0.1, 5)));
        Assert.Null(service.Observe(Frame(start.AddMilliseconds(100), 5, 0.00, 100, 0.1, 0.0, 0.2, 3)));
        Assert.Null(service.Observe(Frame(start.AddMilliseconds(150), 5, 0.05, 120, 0.4, 0.2, -0.1, 4)));
        Assert.Null(service.Observe(Frame(start.AddMilliseconds(200), 5, 0.10, 140, 0.8, 0.5, -0.3, 4)));

        var completed = service.Observe(Frame(start.AddMilliseconds(300), 6, 0.00, 90, 0.0, 0.0, 0.0, 2));

        Assert.NotNull(completed);
        Assert.Equal(5, completed.LapNumber);
        Assert.Equal(2, completed.Trace.Count);
        Assert.Equal(0.10, completed.LapTimeSeconds, 3);
        Assert.Equal(120, completed.AverageSpeedKph, 3);
        Assert.Equal(140, completed.MaxSpeedKph, 3);
        Assert.Equal(100, completed.MinSpeedKph, 3);
        Assert.Equal(0.45, completed.AverageThrottle, 3);
        Assert.Equal(0.5, completed.MaxBrake, 3);
        Assert.Equal(1, completed.GearChanges);
        Assert.Equal(4, completed.TopGear);
        Assert.Equal(3, completed.LowestGear);
        Assert.Equal(1, service.GetStatus().CompletedLapCount);
        Assert.Equal(1, service.GetStatus().BufferedSampleCount);
    }

    [Fact]
    public void Observe_DiscardsFirstPartialLapAndNonSequentialTransitions()
    {
        var service = new TrackingCaptureService();
        var start = DateTimeOffset.UtcNow;
        service.Start(start);

        Assert.Null(service.Observe(Frame(start, 8, 40, 180, .8, 0, 0, 5)));
        Assert.Null(service.Observe(Frame(start.AddSeconds(1), 8, 41, 185, .9, 0, 0, 5)));
        Assert.Null(service.Observe(Frame(start.AddSeconds(2), 1, 0, 100, .2, 0, 0, 2)));
        Assert.Equal(0, service.GetStatus().CompletedLapCount);

        Assert.Null(service.Observe(Frame(start.AddSeconds(3), 1, 1, 110, .3, 0, 0, 2)));
        var completed = service.Observe(Frame(start.AddSeconds(4), 2, 0, 90, .1, 0, 0, 1));

        Assert.NotNull(completed);
        Assert.Equal(1, completed.LapNumber);
    }

    [Fact]
    public void Stop_DiscardsPartialLap()
    {
        var service = new TrackingCaptureService();
        var start = DateTimeOffset.UtcNow;
        service.Start(start);
        service.Observe(Frame(start, 3, 4.2, 180, 0.9, 0.0, 0.1, 6));

        service.Stop();

        Assert.Null(service.Observe(Frame(start.AddSeconds(1), 4, 0, 0, 0, 0, 0, 1)));
        Assert.Equal(0, service.GetStatus().CompletedLapCount);
    }

    private static TrackingTelemetryFrame Frame(
        DateTimeOffset capturedAtUtc,
        int lapNumber,
        double lapElapsedSeconds,
        double speedKph,
        double throttle,
        double brake,
        double steering,
        int gear) => new(
            capturedAtUtc,
            lapNumber,
            lapElapsedSeconds,
            speedKph,
            throttle,
            brake,
            steering,
            gear,
            8000,
            1,
            2,
            3);
}
