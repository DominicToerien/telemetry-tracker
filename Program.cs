using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using telemetry_tracker.Features.TelemetryStatus;
using telemetry_tracker.Telemetry.Lmu;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<LmuTelemetryOptions>(builder.Configuration.GetSection(LmuTelemetryOptions.SectionName));
builder.Services.AddSingleton<LmuTelemetryProvider>();
builder.Services.AddSingleton<ITelemetryStatusQueries>(static sp => sp.GetRequiredService<LmuTelemetryProvider>());
builder.Services.AddSingleton<GetTelemetryStatusHandler>();
builder.Services.AddSingleton<GetTelemetryDebugHandler>();
builder.Services.AddHostedService<LmuTelemetryBackgroundService>();

using var host = builder.Build();

Console.WriteLine("Telemetry Tracker");
Console.WriteLine("Native LMU telemetry client. Press Ctrl+C to exit.");

await host.RunAsync();
