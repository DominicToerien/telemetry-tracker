using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Features.Setups;

public sealed record CreateSetupProposalCommand(Guid SessionId, string Name, string DriverFeedback, Guid? SourceLapId = null);
public sealed record SetupProposalCreationResult(SetupRevisionRecord? Proposal, string? Error);
public sealed record ImportSetupBaselineCommand(Guid SessionId, string FilePath);
public sealed record SetupBaselineImportResult(SetupRevisionRecord? Baseline, string? Error);
public sealed record StoredSvmSetup(string RawText, string FingerprintSha256);

public interface ISetupRevisionStore
{
    Task<SetupProposalCreationResult> CreateProposalAsync(CreateSetupProposalCommand command, CancellationToken cancellationToken);
    Task<SetupBaselineImportResult> ImportBaselineAsync(ImportSetupBaselineCommand command, CancellationToken cancellationToken);
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

    public async Task<SetupBaselineImportResult> ImportBaselineAsync(ImportSetupBaselineCommand command, CancellationToken cancellationToken)
    {
        if (!File.Exists(command.FilePath)) return new(null, $"Setup file was not found: {command.FilePath}");

        var document = await SvmSetupDocument.ReadAsync(command.FilePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(document.VehicleClassSetting)) return new(null, "Setup file does not declare VehicleClassSetting.");

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await db.Sessions.AnyAsync(session => session.Id == command.SessionId, cancellationToken)) return new(null, "Session was not found.");

        var existingBaselines = await db.SetupRevisions
            .Where(item => item.SessionId == command.SessionId && item.Status == "baseline" && item.CarIdentifier == document.VehicleClassSetting)
            .ToListAsync(cancellationToken);
        var parentRevisionId = existingBaselines.OrderByDescending(item => item.CreatedAtUtc).Select(item => (Guid?)item.Id).FirstOrDefault();

        var baseline = new SetupRevisionRecord
        {
            Id = Guid.NewGuid(),
            SessionId = command.SessionId,
            ParentRevisionId = parentRevisionId,
            Name = Path.GetFileNameWithoutExtension(command.FilePath),
            CarIdentifier = document.VehicleClassSetting,
            SetupFormat = "lmu-svm-v1",
            SetupValuesJson = JsonSerializer.Serialize(new StoredSvmSetup(document.WriteUnchanged(), document.FingerprintSha256)),
            Rationale = "Imported immutable LMU baseline setup.",
            Status = "baseline",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        db.SetupRevisions.Add(baseline);
        await db.SaveChangesAsync(cancellationToken);
        return new(baseline, null);
    }

    public async Task<IReadOnlyList<SetupRevisionRecord>> ListAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var revisions = await db.SetupRevisions.AsNoTracking().Where(item => item.SessionId == sessionId).ToListAsync(cancellationToken);
        return revisions.OrderByDescending(item => item.CreatedAtUtc).ToList();
    }
}
