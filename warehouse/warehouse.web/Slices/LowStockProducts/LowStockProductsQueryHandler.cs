using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;

namespace Warehouse.Slices.LowStockProducts;

public record LowStockProductReadModel(string Ean, string Name, string EuVat, double Price, int Quantity);

public record GetAllLowStockProductsQuery : IQuery<IReadOnlyList<LowStockProductReadModel>>;

public class LowStockProductsQueryHandler(SqliteConnectionFactory factory)
    : IQueryHandler<GetAllLowStockProductsQuery, IReadOnlyList<LowStockProductReadModel>>
{
    public async Task<IReadOnlyList<LowStockProductReadModel>> HandleAsync(GetAllLowStockProductsQuery query, CancellationToken ct = default)
    {
        var items = new List<LowStockProductReadModel>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT ean, name, eu_vat, price, quantity FROM low_stock_products WHERE quantity > 0 ORDER BY quantity",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new LowStockProductReadModel(
                Ean: reader.GetString(0),
                Name: reader.GetString(1),
                EuVat: reader.GetString(2),
                Price: reader.GetDouble(3),
                Quantity: reader.GetInt32(4)
            ));
        }

        return items;
    }
}
