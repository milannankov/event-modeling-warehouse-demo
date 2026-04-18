using Warehouse.EventSourcing;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Sale;
using Warehouse.Tests.EventSourcing;

namespace Warehouse.Tests.Slices.InventoryLevels;

public record InventoryLevelsRow(string Ean, string Name, int Quantity);

public class InventoryLevelsProjectionTests
{
    private static ProjectionFixture<InventoryLevelsRow> CreateFixture() =>
        new(ProjectEvent);

    private static Dictionary<string, InventoryLevelsRow> ProjectEvent(
        Dictionary<string, InventoryLevelsRow> state, Event evt)
    {
        switch (evt)
        {
            case ProductCreatedEvent e:
                state[e.Ean] = state.TryGetValue(e.Ean, out var existing)
                    ? existing with { Name = e.Name }
                    : new InventoryLevelsRow(e.Ean, e.Name, 0);
                break;
            case PurchaseCreatedEvent e:
                state[e.Ean] = state.TryGetValue(e.Ean, out var afterPurchase)
                    ? afterPurchase with { Quantity = afterPurchase.Quantity + e.Quantity }
                    : new InventoryLevelsRow(e.Ean, string.Empty, e.Quantity);
                break;
            case ProductSoldEvent e:
                if (state.TryGetValue(e.Ean, out var afterSale))
                    state[e.Ean] = afterSale with { Quantity = afterSale.Quantity - e.Quantity };
                break;
        }
        return state;
    }

    // Config spec: "spec: Inventory Levels - scenario"
    // Given: Product Created (ean=999, name=Water), Purchase Created (ean=999, quantity=200)
    // Then:  Inventory Levels (ean=999, name=Water, quantity=200)
    [Fact]
    public void Spec_InventoryLevels_Scenario()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Water", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.0, Quantity = 200 })
            .ThenExpect("999", new InventoryLevelsRow("999", "Water", 200));
    }

    [Fact]
    public void PurchaseBeforeProductCreated_StillTracksQuantity()
    {
        // Out-of-order arrival (projections are eventually consistent) — name filled in later.
        CreateFixture()
            .Given(
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.0, Quantity = 200 },
                new ProductCreatedEvent { StreamId = "product-999", Name = "Water", Ean = "999" })
            .ThenExpect("999", new InventoryLevelsRow("999", "Water", 200));
    }

    [Fact]
    public void ProductSold_SubtractsFromQuantity()
    {
        CreateFixture()
            .Given(
                new ProductCreatedEvent { StreamId = "product-999", Name = "Water", Ean = "999" },
                new PurchaseCreatedEvent { StreamId = "product-999", Ean = "999", EuVat = "FR44444", Price = 2.0, Quantity = 200 },
                new ProductSoldEvent { StreamId = "product-999", Ean = "999", ClientName = "Client A", SalePrice = 3.0, Quantity = 150 })
            .ThenExpect("999", new InventoryLevelsRow("999", "Water", 50));
    }
}
