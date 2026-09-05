using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using telemetry_tracker.Features.Cli;
using telemetry_tracker.Features.Laps;
using telemetry_tracker.Features.Setups;
using telemetry_tracker.Features.TelemetryStatus;
using telemetry_tracker.Features.TelemetryData;
using telemetry_tracker.Features.Terminal;
using telemetry_tracker.Features.Tracking;
using telemetry_tracker.Infrastructure.Persistence;
using telemetry_tracker.Telemetry.Lmu;

var builder = Host.CreateApplicationBuilder(args);
var isCliCommand = args.Length > 0 && CliCommandRunner.IsCliCommand(args[0]);

builder.Logging.ClearProviders();
if (!isCliCommand) { builder.Logging.AddConsole(); builder.Logging.AddDebug(); }

builder.Services.Configure<LmuTelemetryOptions>(builder.Configuration.GetSection(LmuTelemetryOptions.SectionName));
builder.Services.AddSingleton<LmuTelemetryProvider>();
builder.Services.AddSingleton<TrackingCaptureService>();
builder.Services.AddSingleton<ITrackingControl>(static sp => sp.GetRequiredService<TrackingCaptureService>());
builder.Services.AddSingleton<CompletedLapPersistenceQueue>();
builder.Services.AddSingleton<StartTrackingHandler>();
builder.Services.AddSingleton<StopTrackingHandler>();
builder.Services.AddSingleton<GetTrackingStatusHandler>();
builder.Services.AddSingleton<ILocalLapStore, LocalLapStore>();
builder.Services.AddSingleton<ISetupRevisionStore, SetupRevisionStore>();
builder.Services.AddSingleton<ITelemetryDataQueries, TelemetryDataQueries>();
builder.Services.AddSingleton<CliCommandRunner>();
builder.Services.AddSingleton<WorkspaceContext>();
builder.Services.AddSingleton<TerminalWorkspace>();
builder.Services.TryAddTelemetryTrackerDbContext(builder.Configuration);
builder.Services.AddSingleton<ITelemetryStatusQueries>(static sp => sp.GetRequiredService<LmuTelemetryProvider>());
builder.Services.AddSingleton<GetTelemetryStatusHandler>();
builder.Services.AddSingleton<GetTelemetryDebugHandler>();
builder.Services.AddHostedService<LmuTelemetryBackgroundService>();

using var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TelemetryTrackerDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

var cliResult = await host.Services.GetRequiredService<CliCommandRunner>().RunAsync(args, Console.Out, CancellationToken.None);
if (cliResult is not null) { Environment.ExitCode = cliResult.Value; return; }

Console.WriteLine("Telemetry Tracker");
Console.WriteLine("Native LMU telemetry client. Type /help for commands.");

await host.StartAsync();
await host.Services.GetRequiredService<TerminalWorkspace>().RunAsync(CancellationToken.None);
await host.StopAsync();
