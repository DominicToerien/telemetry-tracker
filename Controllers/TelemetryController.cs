using Microsoft.AspNetCore.Mvc;
using telemetry_tracker.Telemetry;

namespace telemetry_tracker.Controllers;

[ApiController]
[Route("telemetry")]
public sealed class TelemetryController : ControllerBase
{
    private readonly ITelemetryProvider _telemetryProvider;

    public TelemetryController(ITelemetryProvider telemetryProvider)
    {
        _telemetryProvider = telemetryProvider;
    }

    [HttpGet("status")]
    public ActionResult<TelemetryStatusDto> GetStatus() => Ok(_telemetryProvider.GetStatus());

    [HttpGet("debug")]
    public ActionResult<TelemetryDebugDto> GetDebugSnapshot() => Ok(_telemetryProvider.GetDebugSnapshot());
}
