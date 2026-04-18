using Warehouse.EventSourcing;

namespace Warehouse.Slices.CreateVendor;

public record VendorCreatedEvent : Event
{
    public string EuVat { get; init; } = default!;
    public string Name { get; init; } = default!;
}
