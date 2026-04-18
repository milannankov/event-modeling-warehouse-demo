using Warehouse.EventSourcing;

namespace Warehouse.Slices.LowStockAlert;

public record LowStockAlertRaisedEvent : Event
{
    public string Ean { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int Quantity { get; init; }
}
