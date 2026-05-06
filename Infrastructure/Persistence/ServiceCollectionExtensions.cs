using Microsoft.EntityFrameworkCore;

namespace telemetry_tracker.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static bool TryAddTelemetryTrackerDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Supabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        services.AddDbContext<TelemetryTrackerDbContext>(options =>
            options.UseNpgsql(connectionString));

        return true;
    }
}
