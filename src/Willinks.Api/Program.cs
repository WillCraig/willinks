using Willinks.Api.Endpoints;
using Willinks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("DATABASE_URL is not configured.");

var linkService = new LinkService(connectionString);
await linkService.InitializeAsync();
builder.Services.AddSingleton(linkService);

var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/admin", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "index.html"), "text/html"));

// API group with API key auth
var api = app.MapGroup("/api")
    .AddEndpointFilter<ApiKeyFilter>();

api.MapLinksEndpoints();

// Public redirect routes (must be last)
app.MapRedirectEndpoints();

app.Run();

// Inline filter to keep it simple
class ApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expectedKey = configuration["API_KEY"];
        if (string.IsNullOrEmpty(expectedKey))
            return Results.StatusCode(500);

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var providedKey)
            || providedKey != expectedKey)
            return Results.Unauthorized();

        return await next(context);
    }
}
