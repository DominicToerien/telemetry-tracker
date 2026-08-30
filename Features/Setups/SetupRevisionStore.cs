using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Features.Setups;

public sealed record CreateSetupProposalCommand(Guid SessionId, string Name, string DriverFeedback, Guid? SourceLapId = null);

public interface ISetupRevisionStore
{
    Task<SetupRevisionRecord?> CreateProposalAsync(CreateSetupProposalCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<SetupRevisionRecord>> ListAsync(Guid sessionId, CancellationToken cancellationToken);
}

public sealed class SetupRevisionStore(IDbContextFactory<TelemetryTrackerDbContext> dbContextFactory) : ISetupRevisionStore
{
    public async Task<SetupRevisionRecord?> CreateProposalAsync(CreateSetupProposalCommand command, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await db.Sessions.AnyAsync(session => session.Id == command.SessionId, cancellationToken)) return null;
        var proposal = new SetupRevisionRecord
        {
            Id = Guid.NewGuid(), SessionId = command.SessionId, SourceLapId = command.SourceLapId,
            Name = command.Name, SetupValuesJson = "{}", Rationale = command.DriverFeedback,
            Status = "proposed", CreatedAtUtc = DateTimeOffset.UtcNow
        };
        db.SetupRevisions.Add(proposal);
        await db.SaveChangesAsync(cancellationToken);
        return proposal;
    }

    public async Task<IReadOnlyList<SetupRevisionRecord>> ListAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.SetupRevisions.AsNoTracking().Where(item => item.SessionId == sessionId).OrderByDescending(item => item.CreatedAtUtc).ToListAsync(cancellationToken);
    }
}
