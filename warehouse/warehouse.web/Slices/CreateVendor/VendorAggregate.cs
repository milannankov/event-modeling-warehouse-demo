using Warehouse.EventSourcing;

namespace Warehouse.Slices.CreateVendor;

public class VendorAggregate : Aggregate
{
    public string Name { get; private set; } = default!;
    public string EuVat { get; private set; } = default!;
    public bool IsCreated { get; private set; }

    public void CreateVendor(string streamId, string euVat, string name)
    {
        if (IsCreated)
            throw new InvalidOperationException("A vendor with this EU VAT already exists.");

        if (string.IsNullOrWhiteSpace(euVat))
            throw new ArgumentException("EU VAT is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vendor name is required.");

        RaiseEvent(new VendorCreatedEvent
        {
            StreamId = streamId,
            EuVat = euVat,
            Name = name,
        });
    }

    protected override void Apply(Event evt)
    {
        switch (evt)
        {
            case VendorCreatedEvent e:
                StreamId = e.StreamId;
                EuVat = e.EuVat;
                Name = e.Name;
                IsCreated = true;
                break;
        }
    }
}
