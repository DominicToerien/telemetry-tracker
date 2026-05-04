using telemetry_tracker.Telemetry;
using telemetry_tracker.Telemetry.Lmu;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.Configure<LmuTelemetryOptions>(builder.Configuration.GetSection(LmuTelemetryOptions.SectionName));
builder.Services.AddSingleton<LmuTelemetryProvider>();
builder.Services.AddSingleton<ITelemetryProvider>(static sp => sp.GetRequiredService<LmuTelemetryProvider>());
builder.Services.AddHostedService<LmuTelemetryBackgroundService>();
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

app.MapControllers();

app.Run();

public partial class Program;
