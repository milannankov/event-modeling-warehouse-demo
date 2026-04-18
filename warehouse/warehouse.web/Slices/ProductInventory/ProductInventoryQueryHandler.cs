using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;

namespace Warehouse.Slices.ProductInventory;

public record ProductInventoryReadModel(string Ean, string Name, int Quantity);

public record GetAllProductInventoryQuery : IQuery<IReadOnlyList<ProductInventoryReadModel>>;

public class ProductInventoryQueryHandler(SqliteConnectionFactory factory)
    : IQueryHandler<GetAllProductInventoryQuery, IReadOnlyList<ProductInventoryReadModel>>
{
    public async Task<IReadOnlyList<ProductInventoryReadModel>> HandleAsync(GetAllProductInventoryQuery query, CancellationToken ct = default)
    {
        var items = new List<ProductInventoryReadModel>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT ean, name, quantity FROM product_inventory ORDER BY name",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new ProductInventoryReadModel(
                Ean: reader.GetString(0),
                Name: reader.GetString(1),
                Quantity: reader.GetInt32(2)
            ));
        }

        return items;
    }
}
