using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace telemetry_tracker.Tests;

public sealed class TelemetryStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TelemetryStatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task StatusEndpoint_ReturnsDisconnectedPayload()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/telemetry/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"provider\":\"lmu\"", payload, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"connected\":false", payload, StringComparison.OrdinalIgnoreCase);
    }
}
