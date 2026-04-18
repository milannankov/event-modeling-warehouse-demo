using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;
using Warehouse.Slices.NewProduct;

namespace Warehouse.Slices.Products;

public class ProductsProjector(SqliteConnectionFactory factory) : IProjector
{
    public bool CanHandle(Event evt) => evt is ProductCreatedEvent;

    public async Task ProjectAsync(Event evt, CancellationToken ct = default)
    {
        if (evt is not ProductCreatedEvent e)
            return;

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "INSERT INTO products (stream_id, name, ean) VALUES (@streamId, @name, @ean)",
            conn);
        cmd.Parameters.AddWithValue("streamId", e.StreamId);
        cmd.Parameters.AddWithValue("name", e.Name);
        cmd.Parameters.AddWithValue("ean", e.Ean);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
