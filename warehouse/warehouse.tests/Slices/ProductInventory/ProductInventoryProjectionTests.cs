using Warehouse.EventSourcing;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.ProductInventory;
using Warehouse.Slices.Sale;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.ProductInventory;

public class ProductInventoryProjectionTests
{
    private static ProjectionFixture<ProductInventoryReadModel> CreateFixture() =>
        new(ProjectEvent);

    private static Dictionary<string, ProductInventoryReadModel> ProjectEvent(
        Dictionary<string, ProductInventoryReadModel> state, Event evt)
    {
        switch (evt)
        {
            case ProductCreatedEvent e:
                state.TryAdd(e.Ean, new ProductInventoryReadModel(e.Ean, e.Name, 0));
                break;
            case PurchaseCreatedEvent e:
                if (state.TryGetValue(e.Ean, out var afterPurchase))
                    state[e.Ean] = afterPurchase with { Quantity = afterPurchase.Quantity + e.Quantity };
                break;
            case ProductSoldEvent e:
                if (state.TryGetValue(e.Ean, out var afterSale))
                    state[e.Ean] = afterSale with { Quantity = afterSale.Quantity - e.Quantity };
                break;
        }
        return state;
    }

    [Fact]
    public void ProductCreated_SeedsInventoryWithZero()
    {
        CreateFixture()
            .Given(new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" })
            .ThenExpect("999", new ProductInventoryReadModel("999", "Chips", 0));
    }

    [Fact]
    public void PurchaseCreated_AddsToQuantity()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 200 })
            .ThenExpect("999", new ProductInventoryReadModel("999", "Chips", 200));
    }

    [Fact]
    public void ProductSold_SubtractsFromQuantity()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 200 },
                new ProductSoldEvent { StreamId = "product-999", Ean = "999", ClientName = "Client A", SalePrice = 3.0, Quantity = 75 })
            .ThenExpect("999", new ProductInventoryReadModel("999", "Chips", 125));
    }

    [Fact]
    public void CumulativeActivity_MaintainsRunningTotal()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 200 },
                new ProductSoldEvent { StreamId = "product-999", Ean = "999", ClientName = "Client A", SalePrice = 3.0, Quantity = 30 },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "DE111", Price = 1.8, Quantity = 50 },
                new ProductSoldEvent { StreamId = "product-999", Ean = "999", ClientName = "Client B", SalePrice = 3.0, Quantity = 100 })
            .ThenExpect("999", new ProductInventoryReadModel("999", "Chips", 120));
    }
}
