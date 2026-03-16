namespace Willinks.Api.Models;

public class Link
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ClickCount { get; set; }
}
