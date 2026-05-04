using telemetry_tracker.Features.TelemetryStatus;
using telemetry_tracker.Infrastructure.Configuration;
using telemetry_tracker.Infrastructure.Persistence;
using telemetry_tracker.Telemetry.Lmu;

DotEnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
