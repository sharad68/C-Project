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
        Assert.Contains("League of Legends", payload);
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

    [Fact]
    public async Task Editor_Can_Perform_GameTitle_Crud()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsAsync(client, "editor", "Editor@123"));

        var createResponse = await client.PostAsJsonAsync("/api/admin/management/sports", new
        {
            name = "Rocket League",
            slug = "rocket-league",
            description = "Vehicle-based esports league with regional splits and majors.",
            isOlympic = false
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains("Rocket League", createdPayload);

        var createdId = ExtractId(createdPayload);

        var updateResponse = await client.PutAsJsonAsync($"/api/admin/management/sports/{createdId}", new
        {
            name = "Rocket League",
            slug = "rocket-league",
            description = "Tournament-ready vehicle football title with majors and open qualifiers.",
            isOlympic = false
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedPayload = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("Tournament-ready", updatedPayload);

        var deleteResponse = await client.DeleteAsync($"/api/admin/management/sports/{createdId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var sportsPayload = await client.GetStringAsync("/api/sports?page=1&pageSize=50");
        Assert.DoesNotContain("Rocket League", sportsPayload);
    }

    [Fact]
    public async Task Editor_Can_Perform_Series_Crud()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsAsync(client, "editor", "Editor@123"));

        var createResponse = await client.PostAsJsonAsync("/api/admin/management/matches", new
        {
            sportId = 3,
            competitionId = 301,
            seasonId = 3001,
            homeTeamId = 5,
            awayTeamId = 6,
            kickoffUtc = DateTimeOffset.UtcNow.AddDays(3),
            status = "Scheduled",
            homeScore = 0,
            awayScore = 0,
            venue = "Berlin Studio Alpha"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains("Berlin Studio Alpha", createdPayload);

        var createdId = ExtractId(createdPayload);

        var updateResponse = await client.PutAsJsonAsync($"/api/admin/management/matches/{createdId}", new
        {
            sportId = 3,
            competitionId = 301,
            seasonId = 3001,
            homeTeamId = 5,
            awayTeamId = 6,
            kickoffUtc = DateTimeOffset.UtcNow.AddDays(3),
            status = "Live",
            homeScore = 1,
            awayScore = 0,
            venue = "Berlin Studio Alpha"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedPayload = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Live\"", updatedPayload);

        var deleteResponse = await client.DeleteAsync($"/api/admin/management/matches/{createdId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private static int ExtractId(string json)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return document.RootElement.GetProperty("id").GetInt32();
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
