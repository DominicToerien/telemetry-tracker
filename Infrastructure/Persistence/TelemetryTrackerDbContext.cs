using Microsoft.EntityFrameworkCore;

namespace telemetry_tracker.Infrastructure.Persistence;

public sealed class TelemetryTrackerDbContext : DbContext
{
    public TelemetryTrackerDbContext(DbContextOptions<TelemetryTrackerDbContext> options)
        : base(options)
    {
    }
}
