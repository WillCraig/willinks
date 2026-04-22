using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Willinks.Api.Tests;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private TestApplicationFactory(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public static async Task<TestApplicationFactory> CreateAsync()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"willinks-tests-{Guid.NewGuid():N}.db");
        await InitializeDatabaseAsync(databasePath);
        Environment.SetEnvironmentVariable("DATABASE_URL", $"Data Source={databasePath}");
        Environment.SetEnvironmentVariable("API_KEY", "test-secret");
        return new TestApplicationFactory(databasePath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATABASE_URL"] = $"Data Source={DatabasePath}",
                ["API_KEY"] = "test-secret"
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        Environment.SetEnvironmentVariable("DATABASE_URL", null);
        Environment.SetEnvironmentVariable("API_KEY", null);
        if (File.Exists(DatabasePath))
        {
            File.Delete(DatabasePath);
        }
    }

    private static async Task InitializeDatabaseAsync(string databasePath)
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "deploy", "willinks.db.sql");
        var schemaSql = await File.ReadAllTextAsync(schemaPath);

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = schemaSql;
        await command.ExecuteNonQueryAsync();
    }
}
