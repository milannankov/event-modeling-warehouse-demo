using Warehouse.EventSourcing;

namespace Warehouse.Slices.LowStockAlert;

public class RaiseLowStockAlertCommandHandler(IEventStore eventStore)
    : ICommandHandler<RaiseLowStockAlertCommand>
{
    public async Task HandleAsync(RaiseLowStockAlertCommand command, CancellationToken ct = default)
    {
        var streamId = $"low-stock-alert-{command.Ean}";
        var existing = await eventStore.GetEventsByStreamIdAsync(streamId, ct);

        var evt = new LowStockAlertRaisedEvent
        {
            StreamId = streamId,
            Ean = command.Ean,
            Name = command.Name,
            Quantity = command.Quantity,
        };

        await eventStore.AppendToStreamAsync(streamId, existing.Count, new[] { evt }, ct);
    }
}
