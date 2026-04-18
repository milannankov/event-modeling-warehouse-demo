using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;

namespace Warehouse.Slices.Products;

public record ProductReadModel(string StreamId, string Name, string Ean);

public record GetAllProductsQuery : IQuery<IReadOnlyList<ProductReadModel>>;

public class ProductsQueryHandler(SqliteConnectionFactory factory)
    : IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductReadModel>>
{
    public async Task<IReadOnlyList<ProductReadModel>> HandleAsync(GetAllProductsQuery query, CancellationToken ct = default)
    {
        var products = new List<ProductReadModel>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT stream_id, name, ean FROM products ORDER BY name",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            products.Add(new ProductReadModel(
                StreamId: reader.GetString(0),
                Name: reader.GetString(1),
                Ean: reader.GetString(2)
            ));
        }

        return products;
    }
}
