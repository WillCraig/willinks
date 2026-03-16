using Willinks.Api.Services;

namespace Willinks.Api.Endpoints;

public static class RedirectEndpoints
{
    public static void MapRedirectEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{slug}", async (string slug, LinkService links) =>
        {
            var link = await links.GetBySlugAsync(slug);

            if (link is null)
                return Results.NotFound();

            if (link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow)
                return Results.StatusCode(410);

            await links.IncrementClickCountAsync(slug);
            return Results.Redirect(link.Destination);
        });
    }
}
