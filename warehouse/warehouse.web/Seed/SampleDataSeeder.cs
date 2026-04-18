using Microsoft.Data.Sqlite;
using Warehouse.Database;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.CreateVendor;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Sale;

namespace Warehouse.Seed;

public class SampleDataSeeder(
    SqliteConnectionFactory connectionFactory,
    CreateProductCommandHandler createProduct,
    CreateVendorCommandHandler createVendor,
    CreatePurchaseCommandHandler createPurchase,
    CreateProductSaleCommandHandler createSale,
    ILogger<SampleDataSeeder> logger)
{
    private static readonly string[] Tables =
    [
        "events",
        "projector_positions",
        "products",
        "vendors",
        "product_inventory",
        "inventory_levels",
        "low_stock_products",
        "available_products",
    ];

    public async Task ResetAndSeedAsync(CancellationToken ct = default)
    {
        logger.LogWarning("Resetting all data and seeding sample data...");

        await using var conn = await connectionFactory.OpenAsync(ct);
        foreach (var table in Tables)
        {
            await using var cmd = new SqliteCommand($"DELETE FROM {table}", conn);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await using (var resetSeq = new SqliteCommand("DELETE FROM sqlite_sequence", conn))
        {
            await resetSeq.ExecuteNonQueryAsync(ct);
        }

        await createVendor.HandleAsync(new CreateVendorCommand { EuVat = "FR111", Name = "Metro" }, ct);
        await createVendor.HandleAsync(new CreateVendorCommand { EuVat = "FR222", Name = "Carrefour" }, ct);
        await createVendor.HandleAsync(new CreateVendorCommand { EuVat = "BG333", Name = "Billa" }, ct);

        await createProduct.HandleAsync(new CreateProductCommand { Ean = "1001", Name = "Water" }, ct);
        await createProduct.HandleAsync(new CreateProductCommand { Ean = "1002", Name = "Chips" }, ct);
        await createProduct.HandleAsync(new CreateProductCommand { Ean = "1003", Name = "Chocolate" }, ct);
        await createProduct.HandleAsync(new CreateProductCommand { Ean = "1004", Name = "Coffee" }, ct);
        await createProduct.HandleAsync(new CreateProductCommand { Ean = "1005", Name = "Bread" }, ct);
        await createProduct.HandleAsync(new CreateProductCommand { Ean = "1006", Name = "Pasta" }, ct);

        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1001", EuVat = "FR111", Price = 0.50, Quantity = 500 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1001", EuVat = "FR222", Price = 0.55, Quantity = 200 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1002", EuVat = "FR222", Price = 1.20, Quantity = 200 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1002", EuVat = "BG333", Price = 1.30, Quantity = 100 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1003", EuVat = "FR111", Price = 2.50, Quantity = 150 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1004", EuVat = "BG333", Price = 4.00, Quantity = 100 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1005", EuVat = "FR222", Price = 0.80, Quantity = 80 }, ct);
        await createPurchase.HandleAsync(new CreatePurchaseCommand { Ean = "1006", EuVat = "BG333", Price = 1.50, Quantity = 120 }, ct);

        await createSale.HandleAsync(new CreateProductSaleCommand { Ean = "1001", ClientName = "Supermarket Alpha", SalePrice = 0.90, Quantity = 500 }, ct);
        await createSale.HandleAsync(new CreateProductSaleCommand { Ean = "1002", ClientName = "Cafe Beta", SalePrice = 2.00, Quantity = 150 }, ct);
        await createSale.HandleAsync(new CreateProductSaleCommand { Ean = "1003", ClientName = "Hotel Gamma", SalePrice = 4.00, Quantity = 50 }, ct);
        await createSale.HandleAsync(new CreateProductSaleCommand { Ean = "1004", ClientName = "Restaurant Delta", SalePrice = 7.00, Quantity = 93 }, ct);
        await createSale.HandleAsync(new CreateProductSaleCommand { Ean = "1005", ClientName = "Bakery Epsilon", SalePrice = 1.50, Quantity = 75 }, ct);
        await createSale.HandleAsync(new CreateProductSaleCommand { Ean = "1006", ClientName = "Restaurant Zeta", SalePrice = 3.00, Quantity = 118 }, ct);

        logger.LogInformation("Sample data seeded: 3 vendors, 6 products, 8 purchases, 6 sales.");
    }
}
