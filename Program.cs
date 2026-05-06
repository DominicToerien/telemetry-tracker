using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.TelemetryStatus;
using telemetry_tracker.Infrastructure.Persistence;
using telemetry_tracker.Telemetry.Lmu;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddAuthorization();
builder.Services.Configure<LmuTelemetryOptions>(builder.Configuration.GetSection(LmuTelemetryOptions.SectionName));
builder.Services.AddSingleton<LmuTelemetryProvider>();
builder.Services.AddSingleton<ITelemetryStatusQueries>(static sp => sp.GetRequiredService<LmuTelemetryProvider>());
builder.Services.AddSingleton<GetTelemetryStatusHandler>();
builder.Services.AddSingleton<GetTelemetryDebugHandler>();
builder.Services.AddHostedService<LmuTelemetryBackgroundService>();

if (!builder.Services.TryAddTelemetryTrackerDbContext(builder.Configuration))
{
    Console.WriteLine("[Persistence] Supabase connection string not configured; DbContext registration skipped.");
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetService<TelemetryTrackerDbContext>();
    if (dbContext is not null)
    {
        try
        {
            dbContext.Database.Migrate();
            startupLogger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "Database migration failed. Continuing without blocking application startup.");
            Console.WriteLine("[Persistence] Database migration failed: {0}", ex.Message);
        }
    }
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    if (app.Urls.Count == 0)
    {
        startupLogger.LogWarning("Application started but no bound URLs were reported.");
        Console.WriteLine("[Startup] Listening on: (none)");
        return;
    }

    foreach (var url in app.Urls)
    {
        startupLogger.LogInformation("Application listening on: {Url}", url);
        Console.WriteLine("[Startup] Listening on: {0}", url);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapTelemetryStatusEndpoints();

app.Run();

public partial class Program;
