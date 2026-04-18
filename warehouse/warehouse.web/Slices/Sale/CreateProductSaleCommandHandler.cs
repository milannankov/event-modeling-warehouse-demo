using Warehouse.EventSourcing;
using Warehouse.Slices.NewProduct;

namespace Warehouse.Slices.Sale;

public class CreateProductSaleCommandHandler(AggregateRepository repository)
    : ICommandHandler<CreateProductSaleCommand>
{
    public async Task HandleAsync(CreateProductSaleCommand command, CancellationToken ct = default)
    {
        var streamId = $"product-{command.Ean}";

        var aggregate = await repository.LoadAsync<ProductAggregate>(streamId, ct);
        aggregate.CreateProductSale(command.ClientName, command.SalePrice, command.Quantity);
        await repository.SaveAsync(aggregate, ct);
    }
}
