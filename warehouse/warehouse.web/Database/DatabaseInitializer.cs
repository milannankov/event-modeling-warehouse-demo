using Microsoft.Data.Sqlite;

namespace Warehouse.Database;

public class DatabaseInitializer(
    SqliteConnectionFactory factory,
    IHostEnvironment env,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var scriptsDir = Path.Combine(env.ContentRootPath, "Database", "init");
        if (!Directory.Exists(scriptsDir))
            throw new DirectoryNotFoundException($"Init scripts directory not found: {scriptsDir}");

        var scripts = Directory.EnumerateFiles(scriptsDir, "*.sql")
            .OrderBy(p => Path.GetFileName(p), StringComparer.Ordinal)
            .ToList();

        await using var conn = await factory.OpenAsync(ct);

        await using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode=WAL;";
            await pragma.ExecuteNonQueryAsync(ct);
        }

        foreach (var path in scripts)
        {
            var sql = await File.ReadAllTextAsync(path, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(ct);
            logger.LogInformation("Applied init script {Script}", Path.GetFileName(path));
        }
    }
}
