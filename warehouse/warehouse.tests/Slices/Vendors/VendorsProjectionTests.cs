using Warehouse.EventSourcing;
using Warehouse.Slices.CreateVendor;
using Warehouse.Slices.Vendors;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.Vendors;

public class VendorsProjectionTests
{
    private static ProjectionFixture<VendorReadModel> CreateFixture() =>
        new(ProjectEvent);

    private static Dictionary<string, VendorReadModel> ProjectEvent(
        Dictionary<string, VendorReadModel> state, Event evt)
    {
        if (evt is VendorCreatedEvent e)
        {
            state[e.EuVat] = new VendorReadModel(e.StreamId, e.EuVat, e.Name);
        }
        return state;
    }

    [Fact]
    public void VendorCreated_AddsRow()
    {
        CreateFixture()
            .Given(new VendorCreatedEvent { StreamId = "vendor-BG99", EuVat = "BG99", Name = "Metro" })
            .ThenExpect("BG99", new VendorReadModel("vendor-BG99", "BG99", "Metro"));
    }

    [Fact]
    public void MultipleVendors_AreTrackedIndependently()
    {
        CreateFixture()
            .Given(
                new VendorCreatedEvent { StreamId = "vendor-BG99", EuVat = "BG99", Name = "Metro" },
                new VendorCreatedEvent { StreamId = "vendor-FR44", EuVat = "FR44", Name = "Carrefour" })
            .ThenExpect("BG99", new VendorReadModel("vendor-BG99", "BG99", "Metro"))
            .ThenExpect("FR44", new VendorReadModel("vendor-FR44", "FR44", "Carrefour"))
            .ThenExpectCount(2);
    }
}
