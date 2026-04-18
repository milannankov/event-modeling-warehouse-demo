using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Sale;

namespace Warehouse.Slices.InventoryLevels;

public class InventoryLevelsProjector(SqliteConnectionFactory factory) : IProjector
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
                    "INSERT INTO inventory_levels (ean, name, quantity) VALUES (@ean, @name, 0) ON CONFLICT (ean) DO UPDATE SET name = EXCLUDED.name",
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("name", e.Name);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
            case PurchaseCreatedEvent e:
            {
                await using var cmd = new SqliteCommand(
                    "INSERT INTO inventory_levels (ean, quantity, has_purchased) VALUES (@ean, @quantity, 1) ON CONFLICT (ean) DO UPDATE SET quantity = inventory_levels.quantity + @quantity, has_purchased = 1",
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("quantity", e.Quantity);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
            case ProductSoldEvent e:
            {
                await using var cmd = new SqliteCommand(
                    "UPDATE inventory_levels SET quantity = inventory_levels.quantity - @quantity WHERE ean = @ean",
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("quantity", e.Quantity);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
        }
    }
}
