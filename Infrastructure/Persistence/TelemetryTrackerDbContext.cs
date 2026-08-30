using Microsoft.EntityFrameworkCore;

namespace telemetry_tracker.Infrastructure.Persistence;

public sealed class TelemetryTrackerDbContext : DbContext
{
    public TelemetryTrackerDbContext(DbContextOptions<TelemetryTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<SessionRecord> Sessions => Set<SessionRecord>();
    public DbSet<LapSummaryRecord> LapSummaries => Set<LapSummaryRecord>();
    public DbSet<LapTraceRecord> LapTraces => Set<LapTraceRecord>();
    public DbSet<SetupRevisionRecord> SetupRevisions => Set<SetupRevisionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionRecord>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(session => session.Id);
            entity.HasMany(session => session.Laps).WithOne(lap => lap.Session).HasForeignKey(lap => lap.SessionId);
        });

        modelBuilder.Entity<LapSummaryRecord>(entity =>
        {
            entity.ToTable("lap_summaries");
            entity.HasKey(lap => lap.Id);
            entity.HasIndex(lap => new { lap.SessionId, lap.LapNumber }).IsUnique();
            entity.HasOne(lap => lap.Trace).WithOne(trace => trace.LapSummary).HasForeignKey<LapTraceRecord>(trace => trace.LapSummaryId);
        });

        modelBuilder.Entity<LapTraceRecord>(entity =>
        {
            entity.ToTable("lap_traces");
            entity.HasKey(trace => trace.Id);
        });

        modelBuilder.Entity<SetupRevisionRecord>(entity =>
        {
            entity.ToTable("setup_revisions");
            entity.HasKey(revision => revision.Id);
            entity.HasIndex(revision => revision.SessionId);
        });
    }
}
