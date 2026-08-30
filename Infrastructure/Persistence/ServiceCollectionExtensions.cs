using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace telemetry_tracker.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static bool TryAddTelemetryTrackerDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var databasePath = configuration["Persistence:DatabasePath"];
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            databasePath = Path.Combine("data", "telemetry-tracker.db");
        }

        var fullDatabasePath = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullDatabasePath)!);

        services.AddDbContextFactory<TelemetryTrackerDbContext>(options =>
            options.UseSqlite($"Data Source={fullDatabasePath}"));

        return true;
    }
}
