using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;
using Warehouse.Slices.CreateVendor;

namespace Warehouse.Slices.Vendors;

public class VendorsProjector(SqliteConnectionFactory factory) : IProjector
{
    public bool CanHandle(Event evt) => evt is VendorCreatedEvent;

    public async Task ProjectAsync(Event evt, CancellationToken ct = default)
    {
        if (evt is not VendorCreatedEvent e)
            return;

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "INSERT INTO vendors (stream_id, eu_vat, name) VALUES (@streamId, @euVat, @name)",
            conn);
        cmd.Parameters.AddWithValue("streamId", e.StreamId);
        cmd.Parameters.AddWithValue("euVat", e.EuVat);
        cmd.Parameters.AddWithValue("name", e.Name);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
