using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Features.Setups;

public sealed record CreateSetupProposalCommand(Guid SessionId, string Name, string DriverFeedback, Guid? SourceLapId = null);
public sealed record SetupProposalCreationResult(SetupRevisionRecord? Proposal, string? Error);

public interface ISetupRevisionStore
{
    Task<SetupProposalCreationResult> CreateProposalAsync(CreateSetupProposalCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<SetupRevisionRecord>> ListAsync(Guid sessionId, CancellationToken cancellationToken);
}

public sealed class SetupRevisionStore(IDbContextFactory<TelemetryTrackerDbContext> dbContextFactory) : ISetupRevisionStore
{
    public async Task<SetupProposalCreationResult> CreateProposalAsync(CreateSetupProposalCommand command, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await db.Sessions.AnyAsync(session => session.Id == command.SessionId, cancellationToken))
        {
            return new(null, "Session was not found.");
        }

        return new(null, "LMU setup generation requires a validated, car-specific baseline setup and supported setting definitions. No setup was created.");
    }

    public async Task<IReadOnlyList<SetupRevisionRecord>> ListAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.SetupRevisions.AsNoTracking().Where(item => item.SessionId == sessionId).OrderByDescending(item => item.CreatedAtUtc).ToListAsync(cancellationToken);
    }
}
