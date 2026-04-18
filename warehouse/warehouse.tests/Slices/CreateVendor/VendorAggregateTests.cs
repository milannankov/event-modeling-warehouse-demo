using Warehouse.Slices.CreateVendor;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.CreateVendor;

public class VendorAggregateTests
{
    [Fact]
    public void CreateVendor_WithValidData_ProducesVendorCreatedEvent()
    {
        // GIVEN - no prior events
        // WHEN - create vendor
        // THEN - VendorCreatedEvent
        new AggregateFixture<VendorAggregate>()
            .When(a => a.CreateVendor("vendor-BG99", "BG99", "Metro"))
            .ThenExpectEvents(new VendorCreatedEvent
            {
                StreamId = "vendor-BG99",
                EuVat = "BG99",
                Name = "Metro",
            });
    }

    [Fact]
    public void CreateVendor_DuplicateEuVat_ThrowsException()
    {
        // GIVEN - vendor already created (spec: Duplicate euVAT)
        // WHEN - try to create again with same EU VAT
        // THEN - error
        new AggregateFixture<VendorAggregate>()
            .Given(new VendorCreatedEvent { StreamId = "vendor-BG99", EuVat = "BG99", Name = "Metro" })
            .When(a => a.CreateVendor("vendor-BG99", "BG99", "Metro"))
            .ThenExpectException<InvalidOperationException>("already exists");
    }

    [Fact]
    public void CreateVendor_WithEmptyEuVat_ThrowsArgumentException()
    {
        new AggregateFixture<VendorAggregate>()
            .When(a => a.CreateVendor("vendor-", "", "Metro"))
            .ThenExpectException<ArgumentException>("EU VAT is required");
    }

    [Fact]
    public void CreateVendor_WithEmptyName_ThrowsArgumentException()
    {
        new AggregateFixture<VendorAggregate>()
            .When(a => a.CreateVendor("vendor-BG99", "BG99", ""))
            .ThenExpectException<ArgumentException>("Vendor name is required");
    }

    [Fact]
    public void CreateVendor_AppliesStateCorrectly()
    {
        var aggregate = new VendorAggregate();
        aggregate.Load([new VendorCreatedEvent { StreamId = "vendor-BG99", EuVat = "BG99", Name = "Metro" }]);

        Assert.Equal("vendor-BG99", aggregate.StreamId);
        Assert.Equal("BG99", aggregate.EuVat);
        Assert.Equal("Metro", aggregate.Name);
        Assert.True(aggregate.IsCreated);
    }
}
