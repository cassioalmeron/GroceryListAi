using System.Net;
using System.Text.Json;
using GroceryListAi.Tests.Infrastructure;

namespace GroceryListAi.Tests;

public class HealthEndpointTests : IDisposable
{
    private readonly GroceryListAiWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public HealthEndpointTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task HealthEndpointTests_The_health_endpoint_returns_ok_with_the_running_message()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Grocery List API is running", document.RootElement.GetProperty("message").GetString());
    }

    public void Dispose() => _factory.Dispose();
}
