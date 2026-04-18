using Warehouse.EventSourcing;

namespace Warehouse.Slices.Sale;

public record CreateProductSaleCommand : ICommand
{
    public string Ean { get; init; } = default!;
    public string ClientName { get; init; } = default!;
    public double SalePrice { get; init; }
    public int Quantity { get; init; }
}
