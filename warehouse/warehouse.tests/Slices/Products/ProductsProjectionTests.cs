using Warehouse.EventSourcing;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Products;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.Products;

public class ProductsProjectionTests
{
    private static ProjectionFixture<ProductReadModel> CreateFixture() =>
        new(ProjectEvent);

    private static Dictionary<string, ProductReadModel> ProjectEvent(
        Dictionary<string, ProductReadModel> state, Event evt)
    {
        if (evt is ProductCreatedEvent e)
        {
            state[e.Ean] = new ProductReadModel(e.StreamId, e.Name, e.Ean);
        }
        return state;
    }

    [Fact]
    public void ProductCreated_AddsRow()
    {
        CreateFixture()
            .Given(new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" })
            .ThenExpect("999", new ProductReadModel("product-999", "Chips", "999"));
    }

    [Fact]
    public void MultipleProducts_AreTrackedIndependently()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-111", Name = "Widget A", Ean = "111" },
                new ProductCreatedEvent { StreamId = "product-222", Name = "Widget B", Ean = "222" })
            .ThenExpect("111", new ProductReadModel("product-111", "Widget A", "111"))
            .ThenExpect("222", new ProductReadModel("product-222", "Widget B", "222"))
            .ThenExpectCount(2);
    }

    [Fact]
    public void UnrelatedEvents_AreIgnored()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Chips", Ean = "999" },
                new Warehouse.Slices.CreatePurchase.PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR888", Price = 2.0, Quantity = 100 })
            .ThenExpectCount(1);
    }
}
