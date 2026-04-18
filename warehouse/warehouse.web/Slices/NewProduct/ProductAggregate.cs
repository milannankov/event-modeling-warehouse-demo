using Warehouse.EventSourcing;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.Sale;

namespace Warehouse.Slices.NewProduct;

public class ProductAggregate : Aggregate
{
    public string Name { get; private set; } = default!;
    public string Ean { get; private set; } = default!;
    public bool IsCreated { get; private set; }
    public int TotalQuantity { get; private set; }

    public void CreateProduct(string streamId, string name, string ean)
    {
        if (IsCreated)
            throw new InvalidOperationException("A product with this EAN already exists.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.");

        if (string.IsNullOrWhiteSpace(ean))
            throw new ArgumentException("EAN code is required.");

        RaiseEvent(new ProductCreatedEvent
        {
            StreamId = streamId,
            Name = name,
            Ean = ean,
        });
    }

    public void CreatePurchase(string euVat, double price, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        if (TotalQuantity + quantity > 1000)
            throw new InvalidOperationException("Quantity cannot be more than 1000 units.");

        RaiseEvent(new PurchaseCreatedEvent
        {
            StreamId = StreamId,
            Ean = Ean,
            EuVat = euVat,
            Price = price,
            Quantity = quantity,
        });
    }

    public void CreateProductSale(string clientName, double salePrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        if (quantity > TotalQuantity)
            throw new InvalidOperationException("Cannot sell more than available inventory.");

        RaiseEvent(new ProductSoldEvent
        {
            StreamId = StreamId,
            Ean = Ean,
            ClientName = clientName,
            SalePrice = salePrice,
            Quantity = quantity,
        });
    }

    protected override void Apply(Event evt)
    {
        switch (evt)
        {
            case ProductCreatedEvent e:
                StreamId = e.StreamId;
                Name = e.Name;
                Ean = e.Ean;
                IsCreated = true;
                break;
            case PurchaseCreatedEvent e:
                TotalQuantity += e.Quantity;
                break;
            case ProductSoldEvent e:
                TotalQuantity -= e.Quantity;
                break;
        }
    }
}

//  Hydrating the aggregate
//  ───────────────────────
//  Replay every event from the stream through Apply() to rebuild state.
//
//   ┌──────────────────────────────────────┐
//   │ ProductCreated    ean=999  name=Chips│
//   └──────────────────────────────────────┘
//   ┌──────────────────────────────────────┐
//   │ PurchaseCreated   qty=500            │
//   └──────────────────────────────────────┘
//   ┌──────────────────────────────────────┐
//   │ PurchaseCreated   qty=300            │
//   └──────────────────────────────────────┘
//   ┌──────────────────────────────────────┐
//   │ ProductSold       qty=250            │
//   └──────────────────────────────────────┘
//                      │
//                      ▼
//   ┌──────────────────────────────────────┐
//   │ ProductAggregate                     │
//   │ ─────────────────                    │
//   │ IsCreated     = true                 │
//   │ Ean           = "999"                │
//   │ Name          = "Chips"              │
//   │ TotalQuantity = 550                  │
//   └──────────────────────────────────────┘
//
//  The command then runs against this hydrated state.

//  ─────────────────────────────────────────────────────────────────────────────
//
//   Hydrating an aggregate
//   ──────────────────────
//   Aggregates have no "current state" column anywhere. Before a command runs,
//   we rebuild the state in memory by replaying every event ever appended to the
//   aggregate's stream, in order. Only then does the command get to decide.
//
//
//   1. Command arrives
//
//        CreateProductSaleCommand { Ean = "999", Quantity = 100, ... }
//                                         │
//                                         ▼
//         streamId = $"product-{Ean}"   →  "product-999"
//
//
//   2. Load the stream from the event store
//
//        Event Store                 stream: product-999
//        ═══════════                 ═══════════════════
//         #1  ProductCreated   { ean: "999", name: "Chips" }
//         #2  PurchaseCreated  { qty:  500 }
//         #3  PurchaseCreated  { qty:  300 }
//         #4  ProductSold      { qty:  250 }
//
//
//   3. Replay events through Apply() to rebuild state
//
//        new ProductAggregate()            (IsCreated=false, TotalQuantity=0)
//                  │
//                  ├── Apply(#1) ──►  IsCreated=true, Ean="999", Name="Chips"
//                  ├── Apply(#2) ──►  TotalQuantity = 0 + 500  =  500
//                  ├── Apply(#3) ──►  TotalQuantity = 500 + 300 =  800
//                  └── Apply(#4) ──►  TotalQuantity = 800 - 250 =  550
//
//         Hydrated:
//         ┌───────────────────────────────────┐
//         │  ProductAggregate                 │
//         │  ─────────────────                │
//         │  IsCreated     = true             │
//         │  Ean           = "999"            │
//         │  Name          = "Chips"          │
//         │  TotalQuantity = 550              │
//         └───────────────────────────────────┘
//
//
//   4. Only now does the command run against the hydrated state
//
//         CreateProductSale(client, price, quantity: 100)
//                  │
//                  ├── validates invariants (100 ≤ 550 ✓)
//                  │
//                  └── RaiseEvent(new ProductSoldEvent { qty: 100, ... })
//                               │
//                               ▼
//                        appended to the stream as event #5
//
//
//   Invariants live in the aggregate, never in the controller or the DB.
//   The event stream is the single source of truth — everything else
//   (state, read models, projections) is derivable from it.
//
//  ─────────────────────────────────────────────────────────────────────────────
