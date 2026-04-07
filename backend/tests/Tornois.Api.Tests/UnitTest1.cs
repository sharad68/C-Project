using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tornois.Api.Tests;

public sealed class SportsPlatformApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetSports_ReturnsSeededSports()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/sports?page=1&pageSize=5");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Football", payload);
    }

    [Fact]
    public async Task Login_ReturnsJwtToken_ForValidCredentials()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/admin/login", new
        {
            userName = "superadmin",
            password = "Pass@123"
        });

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(result?.Token));
    }

    [Fact]
    public async Task Me_ReturnsUnauthorized_WithoutBearerToken()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsInfrastructureMetadata()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("databaseProvider", payload);
        Assert.Contains("backgroundJobsEnabled", payload);
    }

    [Fact]
    public async Task Superadmin_Can_Create_AdminUser()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsAsync(client, "superadmin", "Pass@123"));

        var response = await client.PostAsJsonAsync("/api/admin/users", new
        {
            userName = "analytics",
            displayName = "Analytics Admin",
            role = "readonly",
            password = "Analytics@123",
            isActive = true
        });

        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("analytics", payload);
    }

    [Fact]
    public async Task Editor_Cannot_Create_AdminUser()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsAsync(client, "editor", "Editor@123"));

        var response = await client.PostAsJsonAsync("/api/admin/users", new
        {
            userName = "blocked-user",
            displayName = "Blocked User",
            role = "readonly",
            password = "Blocked@123",
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<string> LoginAsAsync(HttpClient client, string userName, string password)
    {
        var response = await client.PostAsJsonAsync("/api/admin/login", new { userName, password });
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(result?.Token));
        return result!.Token;
    }

    private sealed record LoginResponse(string Token);
}
