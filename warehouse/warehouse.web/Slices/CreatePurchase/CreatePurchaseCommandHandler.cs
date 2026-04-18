using Warehouse.EventSourcing;
using Warehouse.Slices.NewProduct;

namespace Warehouse.Slices.CreatePurchase;

public class CreatePurchaseCommandHandler(AggregateRepository repository)
    : ICommandHandler<CreatePurchaseCommand>
{
    public async Task HandleAsync(CreatePurchaseCommand command, CancellationToken ct = default)
    {
        var streamId = $"product-{command.Ean}";

        var aggregate = await repository.LoadAsync<ProductAggregate>(streamId, ct);
        aggregate.CreatePurchase(command.EuVat, command.Price, command.Quantity);
        await repository.SaveAsync(aggregate, ct);
    }
}
