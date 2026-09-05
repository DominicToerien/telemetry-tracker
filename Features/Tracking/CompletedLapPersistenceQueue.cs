using Microsoft.Extensions.Logging;
using telemetry_tracker.Features.Laps;

namespace telemetry_tracker.Features.Tracking;

public sealed class CompletedLapPersistenceQueue(
    ILocalLapStore lapStore,
    ILogger<CompletedLapPersistenceQueue> logger)
{
    private readonly Queue<CapturedLap> _pending = [];

    internal int PendingCount => _pending.Count;

    public void Enqueue(CapturedLap lap) => _pending.Enqueue(lap);

    public async Task<IReadOnlyList<CapturedLap>> FlushAsync(CancellationToken cancellationToken)
    {
        var saved = new List<CapturedLap>();

        while (_pending.TryPeek(out var lap))
        {
            try
            {
                await lapStore.SaveAsync(lap, cancellationToken);
                _pending.Dequeue();
                saved.Add(lap);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Unable to persist completed lap {LapNumber} for session {SessionId}. The lap remains queued for retry.",
                    lap.LapNumber,
                    lap.SessionId);
                break;
            }
        }

        return saved;
    }
}
