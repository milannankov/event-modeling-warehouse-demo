using Warehouse.EventSourcing;

namespace Warehouse.Slices.NewProduct;

public record ProductCreatedEvent : Event
{
    public string Name { get; init; } = default!;
    public string Ean { get; init; } = default!;
}
