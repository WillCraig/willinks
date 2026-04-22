using Willinks.Api.Endpoints;
using Willinks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Environment.IsDevelopment()
        ? "Data Source=willinks.db"
        : throw new InvalidOperationException("DATABASE_URL is not configured.");
}

// Convert postgres:// URI to Npgsql key=value format if needed
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
}

// Convert sqlite:// URI to Data Source path
if (connectionString.StartsWith("sqlite://"))
{
    var uri = new Uri(connectionString);
    connectionString = $"Data Source={uri.AbsolutePath.TrimStart('/')}";
}

builder.Services.AddSingleton(new LinkService(connectionString));
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/willinkshealth", () => Results.Ok(new { status = "ok" }));

// Middleware to detect subdomain and route accordingly
app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;
    var isLinksSubdomain = host.StartsWith("links.");
    context.Items["IsLinksSubdomain"] = isLinksSubdomain;
    await next(context);
});

// Razor Pages for admin dashboard (links.willc.pro)
// The Index.cshtml.cs will check subdomain and return 404 if not on links domain
app.MapRazorPages();

// API group with API key auth (willc.pro/api)
var api = app.MapGroup("/api")
    .AddEndpointFilter<ApiKeyFilter>();

api.MapLinksEndpoints();

// Public redirect routes (works on both domains, must be last)
app.MapRedirectEndpoints();

app.Run();

public partial class Program;

// Inline filter to keep it simple
class ApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expectedKey = configuration["API_KEY"];
        if (string.IsNullOrEmpty(expectedKey) && context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            expectedKey = "dev-secret";
        }

        if (string.IsNullOrEmpty(expectedKey))
            return Results.StatusCode(500);

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var providedKey)
            || providedKey != expectedKey)
            return Results.Unauthorized();

        return await next(context);
    }
}
