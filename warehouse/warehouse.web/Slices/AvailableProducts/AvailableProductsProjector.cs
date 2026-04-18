using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Sale;

namespace Warehouse.Slices.AvailableProducts;

public class AvailableProductsProjector(SqliteConnectionFactory factory) : IProjector
{
    public bool CanHandle(Event evt) => evt is ProductCreatedEvent or PurchaseCreatedEvent or ProductSoldEvent;

    public async Task ProjectAsync(Event evt, CancellationToken ct = default)
    {
        await using var conn = await factory.OpenAsync(ct);

        switch (evt)
        {
            case ProductCreatedEvent e:
            {
                await using var cmd = new SqliteCommand(
                    "INSERT INTO available_products (ean, name, available_quantity) VALUES (@ean, @name, 0) ON CONFLICT (ean) DO NOTHING",
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("name", e.Name);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
            case PurchaseCreatedEvent e:
            {
                await using var cmd = new SqliteCommand(
                    "UPDATE available_products SET available_quantity = available_quantity + @quantity WHERE ean = @ean",
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("quantity", e.Quantity);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
            case ProductSoldEvent e:
            {
                await using var cmd = new SqliteCommand(
                    "UPDATE available_products SET available_quantity = available_quantity - @quantity WHERE ean = @ean",
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("quantity", e.Quantity);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
        }
    }
}
