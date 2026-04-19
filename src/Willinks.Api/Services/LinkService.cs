using System.Security.Cryptography;
using Dapper;
using Microsoft.Data.Sqlite;
using Willinks.Api.Models;

namespace Willinks.Api.Services;

public class LinkService(string connectionString)
{
    private SqliteConnection Connect() => new(connectionString);

    public async Task InitializeAsync()
    {
        await using var conn = Connect();
        await conn.ExecuteAsync("PRAGMA journal_mode = WAL;");
        await conn.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS links (
                id           TEXT PRIMARY KEY,
                slug         TEXT NOT NULL UNIQUE,
                destination  TEXT NOT NULL,
                created_at   TEXT NOT NULL,
                expires_at   TEXT,
                click_count  INTEGER NOT NULL DEFAULT 0
            );
            """);
    }

    public async Task<Link?> GetBySlugAsync(string slug)
    {
        await using var conn = Connect();
        return await conn.QuerySingleOrDefaultAsync<Link>(
            "SELECT id, slug, destination, created_at as CreatedAt, expires_at as ExpiresAt, click_count as ClickCount FROM links WHERE slug = @slug",
            new { slug });
    }

    public async Task<IEnumerable<Link>> GetAllAsync()
    {
        await using var conn = Connect();
        return await conn.QueryAsync<Link>(
            "SELECT id, slug, destination, created_at as CreatedAt, expires_at as ExpiresAt, click_count as ClickCount FROM links ORDER BY created_at DESC");
    }

    public async Task<Link> CreateAsync(CreateLinkRequest request)
    {
        string slug = request.Slug ?? await GenerateUniqueSlugAsync();
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        await using var conn = Connect();
        return await conn.QuerySingleAsync<Link>(
            """
            INSERT INTO links (id, slug, destination, created_at, expires_at)
            VALUES (@id, @slug, @destination, @createdAt, @expiresAt)
            RETURNING id, slug, destination, created_at as CreatedAt, expires_at as ExpiresAt, click_count as ClickCount
            """,
            new { id, slug, destination = request.Destination, createdAt, expiresAt = request.ExpiresAt });
    }

    public async Task<bool> DeleteAsync(string slug)
    {
        await using var conn = Connect();
        int rows = await conn.ExecuteAsync("DELETE FROM links WHERE slug = @slug", new { slug });
        return rows > 0;
    }

    public async Task IncrementClickCountAsync(string slug)
    {
        await using var conn = Connect();
        await conn.ExecuteAsync(
            "UPDATE links SET click_count = click_count + 1 WHERE slug = @slug",
            new { slug });
    }

    private async Task<string> GenerateUniqueSlugAsync()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        await using var conn = Connect();
        for (int i = 0; i < 5; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(6);
            var result = new char[6];
            for (int j = 0; j < 6; j++)
                result[j] = chars[bytes[j] % chars.Length];
            string candidate = new string(result);

            var existing = await conn.QuerySingleOrDefaultAsync<string>(
                "SELECT slug FROM links WHERE slug = @slug", new { slug = candidate });
            if (existing is null) return candidate;
        }
        throw new InvalidOperationException("Failed to generate a unique slug after 5 attempts.");
    }
}
