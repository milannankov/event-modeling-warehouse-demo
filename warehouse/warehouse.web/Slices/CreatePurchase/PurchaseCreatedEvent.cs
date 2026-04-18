using Warehouse.EventSourcing;

namespace Warehouse.Slices.CreatePurchase;

public record PurchaseCreatedEvent : Event
{
    public string Ean { get; init; } = default!;
    public string EuVat { get; init; } = default!;
    public double Price { get; init; }
    public int Quantity { get; init; }
}
