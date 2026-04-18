using Microsoft.Data.Sqlite;
using Warehouse.Database;

namespace Warehouse.EventSourcing;

public class ProjectorPositionStore(SqliteConnectionFactory factory)
{
    public async Task<long> GetPositionAsync(string projectorName, CancellationToken ct = default)
    {
        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT last_processed_event_id FROM projector_positions WHERE projector_name = @projectorName",
            conn);
        cmd.Parameters.AddWithValue("projectorName", projectorName);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is long id ? id : 0;
    }

    public async Task<bool> SavePositionAsync(string projectorName, long expectedPosition, long newPosition, CancellationToken ct = default)
    {
        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            """
            INSERT INTO projector_positions (projector_name, last_processed_event_id, updated_at)
            VALUES (@projectorName, @newPosition, strftime('%Y-%m-%dT%H:%M:%fZ','now'))
            ON CONFLICT (projector_name) DO UPDATE
                SET last_processed_event_id = @newPosition, updated_at = strftime('%Y-%m-%dT%H:%M:%fZ','now')
                WHERE projector_positions.last_processed_event_id = @expectedPosition
            """,
            conn);
        cmd.Parameters.AddWithValue("projectorName", projectorName);
        cmd.Parameters.AddWithValue("expectedPosition", expectedPosition);
        cmd.Parameters.AddWithValue("newPosition", newPosition);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
        return rowsAffected > 0;
    }
}
