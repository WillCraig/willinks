using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Willinks.Api.Models;
using Xunit;

namespace Willinks.Api.Tests;

public class WillinksApiTests
{
    [Fact]
    public async Task AdminDashboard_IsAvailableOnLinksSubdomain()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://links.willc.pro")
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Willinks Admin", html);
    }

    [Fact]
    public async Task RootPath_IsNotServedByAppOnMainHost()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://willc.pro")
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task WillinksHealth_ReturnsOk()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://willc.pro")
        });

        var response = await client.GetAsync("/willinkshealth");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ok", body);
    }

    [Fact]
    public async Task SeedLink_IsAvailableInFreshDatabase()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var redirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://willc.pro"),
            AllowAutoRedirect = false
        });

        var response = await redirectClient.GetAsync("/gh");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://github.com/WillCraig", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ApiEndpoints_RequireApiKey()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://willc.pro")
        });

        var response = await client.GetAsync("/api/links");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LinkLifecycle_WorksAgainstTemporarySqliteDatabase()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var client = CreateApiClient(factory, "http://willc.pro");

        var createResponse = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest
        {
            Destination = "https://example.com/articles/testing",
            Slug = "integration-test"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<Link>();
        Assert.NotNull(created);
        Assert.Equal("integration-test", created.Slug);

        var listResponse = await client.GetAsync("/api/links");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var links = await listResponse.Content.ReadFromJsonAsync<List<Link>>();
        Assert.NotNull(links);
        Assert.Contains(links, link => link.Slug == "integration-test");
        Assert.Contains(links, link => link.Slug == "gh");

        using var redirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://willc.pro"),
            AllowAutoRedirect = false
        });
        var redirectResponse = await redirectClient.GetAsync("/integration-test");
        Assert.Equal(HttpStatusCode.Redirect, redirectResponse.StatusCode);
        Assert.Equal("https://example.com/articles/testing", redirectResponse.Headers.Location?.ToString());

        var linkResponse = await client.GetAsync("/api/links/integration-test");
        var fetched = await linkResponse.Content.ReadFromJsonAsync<Link>();
        Assert.NotNull(fetched);
        Assert.Equal(1, fetched.ClickCount);

        var deleteResponse = await client.DeleteAsync("/api/links/integration-test");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await redirectClient.GetAsync("/integration-test");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task ExpiredLinks_ReturnGone()
    {
        await using var factory = await TestApplicationFactory.CreateAsync();
        using var client = CreateApiClient(factory, "http://willc.pro");

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest
        {
            Destination = "https://example.com/expired",
            Slug = "expired-link",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var redirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://willc.pro"),
            AllowAutoRedirect = false
        });
        var redirectResponse = await redirectClient.GetAsync("/expired-link");

        Assert.Equal(HttpStatusCode.Gone, redirectResponse.StatusCode);
    }

    private static HttpClient CreateApiClient(TestApplicationFactory factory, string baseAddress)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(baseAddress)
        });
        client.DefaultRequestHeaders.Add("X-Api-Key", "test-secret");
        return client;
    }
}
