using Microsoft.Extensions.Logging.Abstractions;
using telemetry_tracker.Features.Laps;
using telemetry_tracker.Features.Tracking;

namespace telemetry_tracker.Tests;

public sealed class CompletedLapPersistenceQueueTests
{
    [Fact]
    public async Task FlushAsync_RetainsFailedLapAndRetriesItBeforeLaterLaps()
    {
        var store = new RecordingLapStore { FailuresRemaining = 1 };
        var queue = new CompletedLapPersistenceQueue(store, NullLogger<CompletedLapPersistenceQueue>.Instance);
        var first = CreateLap(1);
        var second = CreateLap(2);
        queue.Enqueue(first);
        queue.Enqueue(second);

        Assert.Empty(await queue.FlushAsync(CancellationToken.None));
        Assert.Equal(2, queue.PendingCount);

        var saved = await queue.FlushAsync(CancellationToken.None);

        Assert.Equal([first, second], saved);
        Assert.Equal([first, first, second], store.Attempts);
        Assert.Equal(0, queue.PendingCount);
    }

    private static CapturedLap CreateLap(int lapNumber) => new(
        Guid.NewGuid(), lapNumber, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
        90, 150, 250, 80, .7, .1, .8, .05, .2, 15, 7, 2,
        [new LapTraceSample(0, 100, .5, 0, 0, 3, 7000, 1, 2, 3)]);

    private sealed class RecordingLapStore : ILocalLapStore
    {
        public int FailuresRemaining { get; set; }
        public List<CapturedLap> Attempts { get; } = [];

        public Task SaveAsync(CapturedLap lap, CancellationToken cancellationToken)
        {
            Attempts.Add(lap);
            if (FailuresRemaining-- > 0)
            {
                throw new IOException("Simulated persistence failure.");
            }

            return Task.CompletedTask;
        }
    }
}
