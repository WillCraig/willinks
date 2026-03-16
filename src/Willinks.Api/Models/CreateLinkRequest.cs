namespace Willinks.Api.Models;

public class CreateLinkRequest
{
    public string Destination { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
