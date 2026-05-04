namespace telemetry_tracker.Features.TelemetryStatus;

public static class Endpoint
{
    public static IEndpointRouteBuilder MapTelemetryStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/telemetry");

        group.MapGet("/status", (GetTelemetryStatusHandler handler) =>
            TypedResults.Ok(handler.Handle(new GetTelemetryStatusQuery())));

        group.MapGet("/debug", (GetTelemetryDebugHandler handler) =>
            TypedResults.Ok(handler.Handle(new GetTelemetryDebugQuery())));

        return app;
    }
}
