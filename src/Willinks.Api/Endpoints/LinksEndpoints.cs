using Willinks.Api.Models;
using Willinks.Api.Services;

namespace Willinks.Api.Endpoints;

public static class LinksEndpoints
{
    public static void MapLinksEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/links", async (LinkService links) =>
            Results.Ok(await links.GetAllAsync()));

        group.MapGet("/links/{slug}", async (string slug, LinkService links) =>
        {
            var link = await links.GetBySlugAsync(slug);
            return link is null ? Results.NotFound() : Results.Ok(link);
        });

        group.MapPost("/links", async (CreateLinkRequest request, LinkService links) =>
        {
            if (string.IsNullOrWhiteSpace(request.Destination))
                return Results.BadRequest("Destination is required.");

            try
            {
                var created = await links.CreateAsync(request);
                return Results.Created($"/api/links/{created.Slug}", created);
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
            {
                return Results.Conflict("Slug already exists.");
            }
        });

        group.MapDelete("/links/{slug}", async (string slug, LinkService links) =>
        {
            bool deleted = await links.DeleteAsync(slug);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
