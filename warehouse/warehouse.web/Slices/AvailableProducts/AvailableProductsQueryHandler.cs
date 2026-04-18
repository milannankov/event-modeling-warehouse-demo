using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;

namespace Warehouse.Slices.AvailableProducts;

public record AvailableProductReadModel(string Ean, string Name, int AvailableQuantity);

public record GetAllAvailableProductsQuery : IQuery<IReadOnlyList<AvailableProductReadModel>>;

public class AvailableProductsQueryHandler(SqliteConnectionFactory factory)
    : IQueryHandler<GetAllAvailableProductsQuery, IReadOnlyList<AvailableProductReadModel>>
{
    public async Task<IReadOnlyList<AvailableProductReadModel>> HandleAsync(GetAllAvailableProductsQuery query, CancellationToken ct = default)
    {
        var items = new List<AvailableProductReadModel>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT ean, name, available_quantity FROM available_products WHERE available_quantity > 0 ORDER BY name",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new AvailableProductReadModel(
                Ean: reader.GetString(0),
                Name: reader.GetString(1),
                AvailableQuantity: reader.GetInt32(2)
            ));
        }

        return items;
    }
}
