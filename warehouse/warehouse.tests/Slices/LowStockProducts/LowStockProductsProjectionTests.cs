using Warehouse.EventSourcing;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.LowStockAlert;
using Warehouse.Slices.LowStockProducts;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Sale;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.LowStockProducts;

public class LowStockProductsProjectionTests
{
    // Mirrors LowStockProductsProjector.LowStockThreshold.
    private const int LowStockThreshold = 10;

    // A single ProjectEvent closure that tracks inventory on the side, mirroring the
    // production projector's cross-read of `inventory_levels`. Each fixture gets its
    // own `inventory` dict, but beware: chaining multiple ThenExpect(key, value) calls
    // re-runs Project(), so multi-assertion tests use the lambda form to ensure a
    // single replay.
    private static ProjectionFixture<LowStockProductReadModel> CreateFixture()
    {
        var inventory = new Dictionary<string, int>();
        return new ProjectionFixture<LowStockProductReadModel>((state, evt) => ProjectEvent(state, evt, inventory));
    }

    private static Dictionary<string, LowStockProductReadModel> ProjectEvent(
        Dictionary<string, LowStockProductReadModel> state,
        Event evt,
        Dictionary<string, int> inventory)
    {
        switch (evt)
        {
            case ProductCreatedEvent e:
                inventory.TryAdd(e.Ean, 0);
                break;

            case PurchaseCreatedEvent e:
            {
                inventory[e.Ean] = inventory.GetValueOrDefault(e.Ean, 0) + e.Quantity;
                if (inventory[e.Ean] >= LowStockThreshold)
                {
                    state.Remove(e.Ean);
                }
                else
                {
                    state[e.Ean] = state.TryGetValue(e.Ean, out var existing)
                        ? existing with { EuVat = e.EuVat, Price = e.Price }
                        : new LowStockProductReadModel(e.Ean, Name: string.Empty, EuVat: e.EuVat, Price: e.Price, Quantity: 0);
                }
                break;
            }

            case ProductSoldEvent e:
                inventory[e.Ean] = inventory.GetValueOrDefault(e.Ean, 0) - e.Quantity;
                break;

            case LowStockAlertRaisedEvent e:
                state[e.Ean] = state.TryGetValue(e.Ean, out var fromAlert)
                    ? fromAlert with { Name = e.Name, Quantity = e.Quantity }
                    : new LowStockProductReadModel(e.Ean, Name: e.Name, EuVat: string.Empty, Price: 0, Quantity: e.Quantity);
                break;
        }
        return state;
    }

    [Fact]
    public void PurchaseCreated_BelowThreshold_SeedsRowWithEmptyName()
    {
        CreateFixture()
            .Given(new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 5 })
            .ThenExpect("999", new LowStockProductReadModel("999", Name: "", EuVat: "FR44444", Price: 2.5, Quantity: 0));
    }

    [Fact]
    public void PurchaseCreated_AtOrAboveThreshold_DoesNotSeedRow()
    {
        CreateFixture()
            .Given(new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 200 })
            .ThenExpectEmpty();
    }

    [Fact]
    public void LowStockAlertRaised_AfterSmallPurchase_CompletesRow()
    {
        CreateFixture()
            .Given(
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 5 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-999", Ean = "999", Name = "Water", Quantity = 5 })
            .ThenExpect("999", new LowStockProductReadModel("999", Name: "Water", EuVat: "FR44444", Price: 2.5, Quantity: 5));
    }

    [Fact]
    public void LatestAlertOverwritesQuantity()
    {
        CreateFixture()
            .Given(
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 5 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-999", Ean = "999", Name = "Water", Quantity = 8 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-999", Ean = "999", Name = "Water", Quantity = 3 })
            .ThenExpect("999", new LowStockProductReadModel("999", Name: "Water", EuVat: "FR44444", Price: 2.5, Quantity: 3));
    }

    [Fact]
    public void PurchaseAfterAlert_BelowThreshold_UpdatesVendorAndPriceKeepsAlertData()
    {
        // A second small purchase keeps inventory below threshold (5 + 3 = 8). The row stays;
        // vendor + price are refreshed; alert-driven name and quantity are preserved.
        CreateFixture()
            .Given(
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 5 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-999", Ean = "999", Name = "Water", Quantity = 5 },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "DE999", Price = 3.1, Quantity = 3 })
            .ThenExpect("999", new LowStockProductReadModel("999", Name: "Water", EuVat: "DE999", Price: 3.1, Quantity: 5));
    }

    [Fact]
    public void PurchaseAfterAlert_RestoreAboveThreshold_RemovesRow()
    {
        // The user-reported scenario: after an alert has been raised for a product, a new
        // purchase brings inventory back above the threshold. The row must disappear so the
        // screen no longer lists the product as low-stock.
        CreateFixture()
            .Given(
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 5 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-999", Ean = "999", Name = "Water", Quantity = 5 },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.5, Quantity = 100 })
            .ThenExpectEmpty();
    }

    [Fact]
    public void MultipleProducts_AreTrackedIndependently()
    {
        CreateFixture()
            .Given(
                new PurchaseCreatedEvent { StreamId = "product-111", Ean = "111", EuVat = "FR44", Price = 1.0, Quantity = 5 },
                new PurchaseCreatedEvent { StreamId = "product-222", Ean = "222", EuVat = "DE99", Price = 2.0, Quantity = 5 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-111", Ean = "111", Name = "Widget A", Quantity = 4 },
                new LowStockAlertRaisedEvent { StreamId = "low-stock-alert-222", Ean = "222", Name = "Widget B", Quantity = 7 })
            .ThenExpect(state =>
            {
                Assert.Equal(2, state.Count);
                Assert.Equal(new LowStockProductReadModel("111", "Widget A", "FR44", 1.0, 4), state["111"]);
                Assert.Equal(new LowStockProductReadModel("222", "Widget B", "DE99", 2.0, 7), state["222"]);
            });
    }
}
