using Warehouse.EventSourcing;

namespace Warehouse.Slices.CreateVendor;

public class CreateVendorCommandHandler(AggregateRepository repository)
    : ICommandHandler<CreateVendorCommand>
{
    public async Task HandleAsync(CreateVendorCommand command, CancellationToken ct = default)
    {
        var streamId = $"vendor-{command.EuVat}";

        var aggregate = await repository.LoadAsync<VendorAggregate>(streamId, ct);
        aggregate.CreateVendor(streamId, command.EuVat, command.Name);
        await repository.SaveAsync(aggregate, ct);
    }
}
