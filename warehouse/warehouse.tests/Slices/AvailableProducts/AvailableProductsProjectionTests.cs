using Warehouse.EventSourcing;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Sale;
using Warehouse.Slices.AvailableProducts;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.AvailableProducts;

public class AvailableProductsProjectionTests
{
    private static ProjectionFixture<AvailableProductReadModel> CreateFixture() =>
        new(ProjectEvent);

    private static Dictionary<string, AvailableProductReadModel> ProjectEvent(
        Dictionary<string, AvailableProductReadModel> state, Event evt)
    {
        switch (evt)
        {
            case ProductCreatedEvent e:
                state[e.Ean] = new AvailableProductReadModel(e.Ean, e.Name, AvailableQuantity: 0);
                break;
            case PurchaseCreatedEvent e:
                if (state.TryGetValue(e.Ean, out var afterPurchase))
                    state[e.Ean] = afterPurchase with { AvailableQuantity = afterPurchase.AvailableQuantity + e.Quantity };
                break;
            case ProductSoldEvent e:
                if (state.TryGetValue(e.Ean, out var afterSale))
                    state[e.Ean] = afterSale with { AvailableQuantity = afterSale.AvailableQuantity - e.Quantity };
                break;
        }
        return state;
    }

    [Fact]
    public void ProductCreated_WithNoInventory_ShowsZeroAvailable()
    {
        // GIVEN a product was created but no purchases made
        // THEN it appears with zero available quantity
        CreateFixture()
            .Given(new ProductCreatedEvent { Ean = "1234567890", Name = "Widget", StreamId = "product-1234567890" })
            .ThenExpect("1234567890", new AvailableProductReadModel("1234567890", "Widget", AvailableQuantity: 0));
    }

    [Fact]
    public void PurchaseCreated_IncreasesAvailableQuantity()
    {
        // GIVEN a product was created and a purchase was made
        // THEN the available quantity reflects the purchase
        CreateFixture()
            .Given(
                new ProductCreatedEvent { Ean = "1234567890", Name = "Widget", StreamId = "product-1234567890" },
                new PurchaseCreatedEvent { Ean = "1234567890", EuVat = "BG123", Price = 10.0, Quantity = 100 })
            .ThenExpect("1234567890", new AvailableProductReadModel("1234567890", "Widget", AvailableQuantity: 100));
    }

    [Fact]
    public void ProductSold_DecreasesAvailableQuantity()
    {
        // GIVEN a product with inventory and a sale was made
        // THEN the available quantity is reduced
        CreateFixture()
            .Given(
                new ProductCreatedEvent { Ean = "1234567890", Name = "Widget", StreamId = "product-1234567890" },
                new PurchaseCreatedEvent { Ean = "1234567890", EuVat = "BG123", Price = 10.0, Quantity = 100 },
                new ProductSoldEvent { Ean = "1234567890", ClientName = "Client A", SalePrice = 15.0, Quantity = 40 })
            .ThenExpect("1234567890", new AvailableProductReadModel("1234567890", "Widget", AvailableQuantity: 60));
    }

    [Fact]
    public void MultiplePurchasesAndSales_TracksAvailableQuantityCorrectly()
    {
        // GIVEN multiple purchases and sales
        // THEN available quantity reflects the cumulative total
        CreateFixture()
            .Given(
                new ProductCreatedEvent { Ean = "1234567890", Name = "Widget", StreamId = "product-1234567890" },
                new PurchaseCreatedEvent { Ean = "1234567890", EuVat = "BG123", Price = 10.0, Quantity = 100 },
                new ProductSoldEvent { Ean = "1234567890", ClientName = "Client A", SalePrice = 15.0, Quantity = 30 },
                new PurchaseCreatedEvent { Ean = "1234567890", EuVat = "BG456", Price = 11.0, Quantity = 50 },
                new ProductSoldEvent { Ean = "1234567890", ClientName = "Client B", SalePrice = 14.0, Quantity = 70 })
            .ThenExpect("1234567890", new AvailableProductReadModel("1234567890", "Widget", AvailableQuantity: 50));
    }

    [Fact]
    public void AllInventorySold_ShowsZeroAvailable()
    {
        // GIVEN all inventory has been sold
        // THEN available quantity is zero
        // Note: the actual query handler filters these out (WHERE available_quantity > 0)
        // but the projection itself still tracks them
        CreateFixture()
            .Given(
                new ProductCreatedEvent { Ean = "1234567890", Name = "Widget", StreamId = "product-1234567890" },
                new PurchaseCreatedEvent { Ean = "1234567890", EuVat = "BG123", Price = 10.0, Quantity = 50 },
                new ProductSoldEvent { Ean = "1234567890", ClientName = "Client A", SalePrice = 15.0, Quantity = 50 })
            .ThenExpect("1234567890", new AvailableProductReadModel("1234567890", "Widget", AvailableQuantity: 0));
    }

    [Fact]
    public void MultipleProducts_TrackedIndependently()
    {
        // GIVEN two products with different sales histories
        // THEN each tracks its own available quantity
        CreateFixture()
            .Given(
                new ProductCreatedEvent { Ean = "111", Name = "Widget A", StreamId = "product-111" },
                new ProductCreatedEvent { Ean = "222", Name = "Widget B", StreamId = "product-222" },
                new PurchaseCreatedEvent { Ean = "111", EuVat = "BG123", Price = 10.0, Quantity = 100 },
                new PurchaseCreatedEvent { Ean = "222", EuVat = "BG123", Price = 20.0, Quantity = 50 },
                new ProductSoldEvent { Ean = "111", ClientName = "Client A", SalePrice = 15.0, Quantity = 30 })
            .ThenExpect("111", new AvailableProductReadModel("111", "Widget A", AvailableQuantity: 70))
            .ThenExpect("222", new AvailableProductReadModel("222", "Widget B", AvailableQuantity: 50))
            .ThenExpectCount(2);
    }
}
