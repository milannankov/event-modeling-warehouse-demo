using Microsoft.Data.Sqlite;

namespace Warehouse.Database;

public class SqliteConnectionFactory(IConfiguration config)
{
    private readonly string _connectionString = config.GetConnectionString("warehousedb")
        ?? throw new InvalidOperationException("Connection string 'warehousedb' is missing.");

    public async Task<SqliteConnection> OpenAsync(CancellationToken ct = default)
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
