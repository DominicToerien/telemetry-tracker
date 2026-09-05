using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using telemetry_tracker.Infrastructure.Persistence;

namespace telemetry_tracker.Features.Setups;

public sealed record CreateSetupProposalCommand(Guid SessionId, string Name, string DriverFeedback, Guid? SourceLapId = null);
public sealed record SetupProposalCreationResult(SetupRevisionRecord? Proposal, string? Error);
public sealed record ImportSetupBaselineCommand(Guid SessionId, string FilePath);
public sealed record SetupBaselineImportResult(SetupRevisionRecord? Baseline, string? Error);
public sealed record CreateSetupModificationCommand(Guid SourceRevisionId, Guid SourceLapId, string Name, string DriverFeedback, IReadOnlyCollection<SetupSettingChange> Changes);
public sealed record SetupModificationCreationResult(SetupRevisionRecord? Proposal, IReadOnlyList<AppliedSetupSettingChange> Changes, string? Error);
public sealed record StoredSvmSetup(string RawText, string RawContentBase64, string FingerprintSha256);
public sealed record SetupRevisionSummary(Guid Id, Guid SessionId, Guid? ParentRevisionId, string Name, string? CarIdentifier, string? SetupFormat, string Status, string? FingerprintSha256, DateTimeOffset CreatedAtUtc);
public sealed record SetupRevisionDetails(SetupRevisionSummary Summary, IReadOnlyList<SvmSetting> Settings);
public sealed record SetupSettingDifference(string? Section, string Name, string? FirstValue, string? FirstComment, string? SecondValue, string? SecondComment);
public sealed record SetupComparison(Guid FirstId, Guid SecondId, string CarIdentifier, IReadOnlyList<SetupSettingDifference> Differences);

public interface ISetupRevisionStore
{
    Task<SetupProposalCreationResult> CreateProposalAsync(CreateSetupProposalCommand command, CancellationToken cancellationToken);
    Task<SetupBaselineImportResult> ImportBaselineAsync(ImportSetupBaselineCommand command, CancellationToken cancellationToken);
    Task<SetupModificationCreationResult> CreateModificationAsync(CreateSetupModificationCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<SetupRevisionSummary>> ListAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<SetupRevisionDetails?> GetAsync(Guid revisionId, CancellationToken cancellationToken);
    Task<SetupComparison?> CompareAsync(Guid firstId, Guid secondId, CancellationToken cancellationToken);
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
        var duplicate = existingBaselines.FirstOrDefault(item => TryReadStoredSvm(item)?.FingerprintSha256 == document.FingerprintSha256);
        if (duplicate is not null) return new(duplicate, null);

        var parentRevisionId = existingBaselines.OrderByDescending(item => item.CreatedAtUtc).Select(item => (Guid?)item.Id).FirstOrDefault();

        var baseline = new SetupRevisionRecord
        {
            Id = Guid.NewGuid(),
            SessionId = command.SessionId,
            ParentRevisionId = parentRevisionId,
            Name = Path.GetFileNameWithoutExtension(command.FilePath),
            CarIdentifier = document.VehicleClassSetting,
            SetupFormat = "lmu-svm-v1",
            SetupValuesJson = JsonSerializer.Serialize(new StoredSvmSetup(document.SourceText, Convert.ToBase64String(document.WriteUnchangedBytes()), document.FingerprintSha256)),
            Rationale = "Imported immutable LMU baseline setup.",
            Status = "baseline",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        db.SetupRevisions.Add(baseline);
        await db.SaveChangesAsync(cancellationToken);
        return new(baseline, null);
    }

    public async Task<SetupModificationCreationResult> CreateModificationAsync(CreateSetupModificationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) return new(null, [], "A proposal name is required.");
        if (string.IsNullOrWhiteSpace(command.DriverFeedback)) return new(null, [], "Driver feedback is required.");

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sourceRevision = await db.SetupRevisions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == command.SourceRevisionId, cancellationToken);
        if (sourceRevision is null) return new(null, [], "Source setup revision was not found.");
        var sourceLap = await db.LapSummaries.AsNoTracking()
            .Where(lap => lap.Id == command.SourceLapId && lap.SessionId == sourceRevision.SessionId)
            .Select(lap => new { lap.Id, VehicleName = lap.Session!.VehicleName })
            .SingleOrDefaultAsync(cancellationToken);
        if (sourceLap is null)
        {
            return new(null, [], "Source lap was not found in the baseline setup's session.");
        }
        if (string.IsNullOrWhiteSpace(sourceRevision.CarIdentifier) ||
            !string.Equals(sourceLap.VehicleName, sourceRevision.CarIdentifier, StringComparison.Ordinal))
        {
            return new(null, [], $"Source lap car '{sourceLap.VehicleName ?? "<missing>"}' does not exactly match setup car '{sourceRevision.CarIdentifier ?? "<missing>"}'.");
        }

        var storedSource = TryReadStoredSvm(sourceRevision);
        if (storedSource is null) return new(null, [], "Source setup revision does not contain a valid LMU setup artifact.");

        byte[] sourceBytes;
        try
        {
            sourceBytes = Convert.FromBase64String(storedSource.RawContentBase64);
        }
        catch (FormatException)
        {
            return new(null, [], "Source setup revision contains invalid setup bytes.");
        }

        var modification = BmwM4SetupModifier.Modify(sourceBytes, command.Changes);
        if (modification.Error is not null) return new(null, [], modification.Error);

        var content = modification.Content!;
        var document = SvmSetupDocument.Parse(content);
        var proposal = new SetupRevisionRecord
        {
            Id = Guid.NewGuid(),
            SessionId = sourceRevision.SessionId,
            SourceLapId = command.SourceLapId,
            ParentRevisionId = sourceRevision.Id,
            Name = command.Name.Trim(),
            CarIdentifier = document.VehicleClassSetting,
            SetupFormat = "lmu-svm-v1",
            SetupValuesJson = JsonSerializer.Serialize(new StoredSvmSetup(
                document.SourceText,
                Convert.ToBase64String(content),
                document.FingerprintSha256)),
            Rationale = $"Driver feedback: {command.DriverFeedback.Trim()}. Changes: {string.Join("; ", modification.Changes.Select(change => $"[{change.Section}] {change.Name}: {change.PreviousValue} -> {change.Value}"))}",
            Status = "proposal",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        db.SetupRevisions.Add(proposal);
        await db.SaveChangesAsync(cancellationToken);
        return new(proposal, modification.Changes, null);
    }

    public async Task<IReadOnlyList<SetupRevisionSummary>> ListAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var revisions = await db.SetupRevisions.AsNoTracking().Where(item => item.SessionId == sessionId).ToListAsync(cancellationToken);
        return revisions.OrderByDescending(item => item.CreatedAtUtc).Select(ToSummary).ToList();
    }

    public async Task<SetupRevisionDetails?> GetAsync(Guid revisionId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var revision = await db.SetupRevisions.AsNoTracking().SingleOrDefaultAsync(item => item.Id == revisionId, cancellationToken);
        if (revision is null || revision.SetupFormat?.Equals("lmu-svm-v1", StringComparison.Ordinal) != true) return null;
        var stored = TryReadStoredSvm(revision);
        return stored is null ? null : new SetupRevisionDetails(ToSummary(revision), ParseStored(stored).Settings);
    }

    public async Task<SetupComparison?> CompareAsync(Guid firstId, Guid secondId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var revisions = await db.SetupRevisions.AsNoTracking().Where(item => item.Id == firstId || item.Id == secondId).ToListAsync(cancellationToken);
        if (revisions.Count != 2) return null;
        var first = revisions.Single(item => item.Id == firstId);
        var second = revisions.Single(item => item.Id == secondId);
        if (string.IsNullOrWhiteSpace(first.CarIdentifier) || !string.Equals(first.CarIdentifier, second.CarIdentifier, StringComparison.Ordinal)) return null;

        var firstStored = TryReadStoredSvm(first);
        var secondStored = TryReadStoredSvm(second);
        if (firstStored is null || secondStored is null) return null;

        var firstSettings = ParseStored(firstStored).Settings.ToDictionary(setting => (setting.Section, setting.Name));
        var secondSettings = ParseStored(secondStored).Settings.ToDictionary(setting => (setting.Section, setting.Name));
        var keys = firstSettings.Keys.Union(secondSettings.Keys).OrderBy(key => key.Section).ThenBy(key => key.Name);
        var differences = keys
            .Select(key =>
            {
                firstSettings.TryGetValue(key, out var firstSetting);
                secondSettings.TryGetValue(key, out var secondSetting);
                return new SetupSettingDifference(key.Section, key.Name, firstSetting?.Value, firstSetting?.Comment, secondSetting?.Value, secondSetting?.Comment);
            })
            .Where(difference => difference.FirstValue != difference.SecondValue || difference.FirstComment != difference.SecondComment)
            .ToList();

        return new SetupComparison(firstId, secondId, first.CarIdentifier, differences);
    }

    private static StoredSvmSetup? TryReadStoredSvm(SetupRevisionRecord revision)
    {
        try
        {
            var stored = JsonSerializer.Deserialize<StoredSvmSetup>(revision.SetupValuesJson);
            return string.IsNullOrEmpty(stored?.RawText) || string.IsNullOrEmpty(stored.RawContentBase64) || string.IsNullOrEmpty(stored.FingerprintSha256) ? null : stored;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static SvmSetupDocument ParseStored(StoredSvmSetup stored) =>
        SvmSetupDocument.Parse(Convert.FromBase64String(stored.RawContentBase64));

    private static SetupRevisionSummary ToSummary(SetupRevisionRecord revision) => new(
        revision.Id,
        revision.SessionId,
        revision.ParentRevisionId,
        revision.Name,
        revision.CarIdentifier,
        revision.SetupFormat,
        revision.Status,
        TryReadStoredSvm(revision)?.FingerprintSha256,
        revision.CreatedAtUtc);
}
