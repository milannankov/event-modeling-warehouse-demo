using Microsoft.Data.Sqlite;
using Warehouse.Database;
using Warehouse.EventSourcing;
using Warehouse.Slices.CreatePurchase;

namespace Warehouse.Slices.LowStockProducts;

public class LowStockProductsProjector(SqliteConnectionFactory factory) : IProjector
{
    // Mirrors LowStockAlertProcessor.LowStockThreshold — keep in sync.
    private const int LowStockThreshold = 10;

    public bool CanHandle(Event evt) => evt is PurchaseCreatedEvent or LowStockAlert.LowStockAlertRaisedEvent;

    public async Task ProjectAsync(Event evt, CancellationToken ct = default)
    {
        await using var conn = await factory.OpenAsync(ct);

        switch (evt)
        {
            case PurchaseCreatedEvent e:
            {
                var currentQuantity = await GetInventoryAsync(conn, e.Ean, ct);
                if (currentQuantity >= LowStockThreshold)
                {
                    // Restock cleared the low-stock condition — drop any existing row.
                    await using var deleteCmd = new SqliteCommand(
                        "DELETE FROM low_stock_products WHERE ean = @ean",
                        conn);
                    deleteCmd.Parameters.AddWithValue("ean", e.Ean);
                    await deleteCmd.ExecuteNonQueryAsync(ct);
                }
                else
                {
                    // Still low: remember latest vendor + price in case an alert follows.
                    await using var upsertCmd = new SqliteCommand(
                        """
                        INSERT INTO low_stock_products (ean, eu_vat, price, quantity)
                        VALUES (@ean, @eu_vat, @price, 0)
                        ON CONFLICT (ean) DO UPDATE SET
                            eu_vat = EXCLUDED.eu_vat,
                            price = EXCLUDED.price
                        """,
                        conn);
                    upsertCmd.Parameters.AddWithValue("ean", e.Ean);
                    upsertCmd.Parameters.AddWithValue("eu_vat", e.EuVat);
                    upsertCmd.Parameters.AddWithValue("price", e.Price);
                    await upsertCmd.ExecuteNonQueryAsync(ct);
                }
                break;
            }
            case LowStockAlert.LowStockAlertRaisedEvent e:
            {
                await using var cmd = new SqliteCommand(
                    """
                    INSERT INTO low_stock_products (ean, name, quantity)
                    VALUES (@ean, @name, @quantity)
                    ON CONFLICT (ean) DO UPDATE SET
                        quantity = EXCLUDED.quantity,
                        name = EXCLUDED.name
                    """,
                    conn);
                cmd.Parameters.AddWithValue("ean", e.Ean);
                cmd.Parameters.AddWithValue("name", e.Name);
                cmd.Parameters.AddWithValue("quantity", e.Quantity);
                await cmd.ExecuteNonQueryAsync(ct);
                break;
            }
        }
    }

    private static async Task<int> GetInventoryAsync(SqliteConnection conn, string ean, CancellationToken ct)
    {
        await using var cmd = new SqliteCommand(
            "SELECT quantity FROM inventory_levels WHERE ean = @ean",
            conn);
        cmd.Parameters.AddWithValue("ean", ean);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is long q ? (int)q : 0;
    }
}
