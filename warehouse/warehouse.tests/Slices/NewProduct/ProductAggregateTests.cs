using Warehouse.Slices.NewProduct;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.Sale;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.NewProduct;

public class ProductAggregateTests
{
    // ── New Product slice ──

    [Fact]
    public void CreateProduct_WithValidData_ProducesProductCreatedEvent()
    {
        // GIVEN - no prior events
        var streamId = "product-999";

        // WHEN / THEN
        new AggregateFixture<ProductAggregate>()
            .When(a => a.CreateProduct(streamId, "Widget A", "999"))
            .ThenExpectEvents(new ProductCreatedEvent
            {
                StreamId = streamId,
                Name = "Widget A",
                Ean = "999",
            });
    }

    [Fact]
    public void CreateProduct_DuplicateEan_ThrowsException()
    {
        // GIVEN - product already created (spec: Duplicate EAN)
        // WHEN - try to create again with same EAN
        // THEN - error
        new AggregateFixture<ProductAggregate>()
            .Given(new ProductCreatedEvent { StreamId = "product-999", Name = "Product", Ean = "999" })
            .When(a => a.CreateProduct("product-999", "Product", "999"))
            .ThenExpectException<InvalidOperationException>("already exists");
    }

    [Fact]
    public void CreateProduct_WithEmptyName_ThrowsArgumentException()
    {
        new AggregateFixture<ProductAggregate>()
            .When(a => a.CreateProduct("product-999", "", "999"))
            .ThenExpectException<ArgumentException>("Product name is required");
    }

    [Fact]
    public void CreateProduct_WithEmptyEan_ThrowsArgumentException()
    {
        new AggregateFixture<ProductAggregate>()
            .When(a => a.CreateProduct("product-999", "Widget A", ""))
            .ThenExpectException<ArgumentException>("EAN code is required");
    }

    [Fact]
    public void CreateProduct_AppliesStateCorrectly()
    {
        var aggregate = new ProductAggregate();
        aggregate.Load([new ProductCreatedEvent { StreamId = "product-999", Name = "Widget B", Ean = "999" }]);

        Assert.Equal("product-999", aggregate.StreamId);
        Assert.Equal("Widget B", aggregate.Name);
        Assert.Equal("999", aggregate.Ean);
        Assert.True(aggregate.IsCreated);
        Assert.Equal(0, aggregate.TotalQuantity);
    }

    // ── Create Purchase slice ──

    [Fact]
    public void CreatePurchase_HappyPath_ProducesPurchaseCreatedEvent()
    {
        // GIVEN - product exists (spec: happy path)
        // WHEN - create purchase
        // THEN - PurchaseCreatedEvent
        new AggregateFixture<ProductAggregate>()
            .Given(new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" })
            .When(a => a.CreatePurchase("FR888", 2.00, 500))
            .ThenExpectEvents(new PurchaseCreatedEvent
            {
                StreamId = "product-999",
                Ean = "999",
                EuVat = "FR888",
                Price = 2.00,
                Quantity = 500,
            });
    }

    [Fact]
    public void CreatePurchase_QuantityExceeded_ThrowsException()
    {
        // GIVEN - product exists + 900 units already purchased (spec: Quantity Exceeded)
        // WHEN - try to purchase 200 more (total would be 1100)
        // THEN - error
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 900 })
            .When(a => a.CreatePurchase("FR888", 2.0, 200))
            .ThenExpectException<InvalidOperationException>("1000 units");
    }

    [Fact]
    public void CreatePurchase_ExactlyAt1000_Succeeds()
    {
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 500 })
            .When(a => a.CreatePurchase("FR888", 2.0, 500))
            .ThenExpectEvents(new PurchaseCreatedEvent
            {
                StreamId = "product-999",
                Ean = "999",
                EuVat = "FR888",
                Price = 2.0,
                Quantity = 500,
            });
    }

    [Fact]
    public void CreatePurchase_TracksQuantityAcrossMultiplePurchases()
    {
        // 3 purchases of 400 = 1200 total, should fail on 3rd
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 400 },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "DE111", Price = 1.5, Quantity = 400 })
            .When(a => a.CreatePurchase("FR888", 2.0, 400))
            .ThenExpectException<InvalidOperationException>("1000 units");
    }

    // ── Sale slice ──

    [Fact]
    public void CreateProductSale_HappyPath_ProducesProductSoldEvent()
    {
        // GIVEN - product exists + 100 units purchased (spec: happy path)
        // WHEN - sell 50
        // THEN - ProductSoldEvent
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 100 })
            .When(a => a.CreateProductSale("Client 1", 3.0, 50))
            .ThenExpectEvents(new ProductSoldEvent
            {
                StreamId = "product-999",
                Ean = "999",
                ClientName = "Client 1",
                SalePrice = 3.0,
                Quantity = 50,
            });
    }

    [Fact]
    public void CreateProductSale_ExceedsInventory_ThrowsException()
    {
        // GIVEN - product exists + 100 units purchased (spec: Exceeds Inventory)
        // WHEN - try to sell 500
        // THEN - error
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 100 })
            .When(a => a.CreateProductSale("Client 1", 3.0, 500))
            .ThenExpectException<InvalidOperationException>("more than available inventory");
    }

    [Fact]
    public void CreateProductSale_SellExactInventory_Succeeds()
    {
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 100 })
            .When(a => a.CreateProductSale("Client 1", 3.0, 100))
            .ThenExpectEvents(new ProductSoldEvent
            {
                StreamId = "product-999",
                Ean = "999",
                ClientName = "Client 1",
                SalePrice = 3.0,
                Quantity = 100,
            });
    }

    [Fact]
    public void QuantityTracking_PurchasesAndSales_MaintainsCorrectTotal()
    {
        // Purchase 100, sell 60, then try to sell 50 more (only 40 left)
        new AggregateFixture<ProductAggregate>()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 100 },
                new ProductSoldEvent { StreamId = "product-999", Ean = "999", ClientName = "Client 1", SalePrice = 3.0, Quantity = 60 })
            .When(a => a.CreateProductSale("Client 2", 3.0, 50))
            .ThenExpectException<InvalidOperationException>("more than available inventory");
    }
}
