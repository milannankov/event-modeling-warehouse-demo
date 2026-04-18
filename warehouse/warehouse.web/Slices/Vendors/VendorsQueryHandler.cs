using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;

namespace Warehouse.Slices.Vendors;

public record VendorReadModel(string StreamId, string EuVat, string Name);

public record GetAllVendorsQuery : IQuery<IReadOnlyList<VendorReadModel>>;

public class VendorsQueryHandler(SqliteConnectionFactory factory)
    : IQueryHandler<GetAllVendorsQuery, IReadOnlyList<VendorReadModel>>
{
    public async Task<IReadOnlyList<VendorReadModel>> HandleAsync(GetAllVendorsQuery query, CancellationToken ct = default)
    {
        var vendors = new List<VendorReadModel>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT stream_id, eu_vat, name FROM vendors ORDER BY name",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            vendors.Add(new VendorReadModel(
                StreamId: reader.GetString(0),
                EuVat: reader.GetString(1),
                Name: reader.GetString(2)
            ));
        }

        return vendors;
    }
}
