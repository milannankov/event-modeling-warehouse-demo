using Warehouse.EventSourcing;

namespace Warehouse.Slices.CreateVendor;

public record CreateVendorCommand : ICommand
{
    public string EuVat { get; init; } = default!;
    public string Name { get; init; } = default!;
}
