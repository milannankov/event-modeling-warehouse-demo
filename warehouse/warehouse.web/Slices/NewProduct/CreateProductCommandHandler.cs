using Warehouse.EventSourcing;

namespace Warehouse.Slices.NewProduct;

public class CreateProductCommandHandler(AggregateRepository repository)
    : ICommandHandler<CreateProductCommand>
{
    public async Task HandleAsync(CreateProductCommand command, CancellationToken ct = default)
    {
        var streamId = $"product-{command.Ean}";

        var product = await repository.LoadAsync<ProductAggregate>(streamId, ct);
        product.CreateProduct(streamId, command.Name, command.Ean);
        await repository.SaveAsync(product, ct);
    }
}
