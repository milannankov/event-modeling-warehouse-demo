using Warehouse.EventSourcing;
using Warehouse.Database;
using Warehouse.Slices.NewProduct;
using Warehouse.Slices.Products;
using Warehouse.Slices.CreateVendor;
using Warehouse.Slices.Vendors;
using Warehouse.Slices.CreatePurchase;
using Warehouse.Slices.Sale;
using Warehouse.Slices.ProductInventory;
using Warehouse.Slices.InventoryLevels;
using Warehouse.Slices.LowStockAlert;
using Warehouse.Slices.LowStockProducts;
using Warehouse.Slices.AvailableProducts;
using Warehouse.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SqliteConnectionFactory>();
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton<IEventStore, SqliteEventStore>();
builder.Services.AddSingleton<AggregateRepository>();

// Slice: New Product (State Change)
builder.Services.AddSingleton<CreateProductCommandHandler>();

// Slice: Products (State View)
builder.Services.AddSingleton<ProductsProjector>();
builder.Services.AddSingleton<IProjector>(sp => sp.GetRequiredService<ProductsProjector>());
builder.Services.AddSingleton<ProductsQueryHandler>();

// Slice: Create Vendor (State Change)
builder.Services.AddSingleton<CreateVendorCommandHandler>();

// Slice: Vendors (State View)
builder.Services.AddSingleton<VendorsProjector>();
builder.Services.AddSingleton<IProjector>(sp => sp.GetRequiredService<VendorsProjector>());
builder.Services.AddSingleton<VendorsQueryHandler>();

// Slice: Create Purchase (State Change)
builder.Services.AddSingleton<CreatePurchaseCommandHandler>();

// Slice: Product Inventory (State View)
builder.Services.AddSingleton<ProductInventoryProjector>();
builder.Services.AddSingleton<IProjector>(sp => sp.GetRequiredService<ProductInventoryProjector>());
builder.Services.AddSingleton<ProductInventoryQueryHandler>();

// Slice: Sale (State Change)
builder.Services.AddSingleton<CreateProductSaleCommandHandler>();

// Slice: Available Products (State View - for sale screen)
builder.Services.AddSingleton<AvailableProductsProjector>();
builder.Services.AddSingleton<IProjector>(sp => sp.GetRequiredService<AvailableProductsProjector>());
builder.Services.AddSingleton<AvailableProductsQueryHandler>();

// Slice: Inventory Levels (State View - feeds automation)
builder.Services.AddSingleton<InventoryLevelsProjector>();
builder.Services.AddSingleton<IProjector>(sp => sp.GetRequiredService<InventoryLevelsProjector>());

// Slice: Low Stock Alert (Automation + State Change)
builder.Services.AddSingleton<RaiseLowStockAlertCommandHandler>();
builder.Services.AddHostedService<LowStockAlertProcessor>();

// Slice: Low Stock Products (State View)
builder.Services.AddSingleton<LowStockProductsProjector>();
builder.Services.AddSingleton<IProjector>(sp => sp.GetRequiredService<LowStockProductsProjector>());
builder.Services.AddSingleton<LowStockProductsQueryHandler>();

builder.Services.AddSingleton<ProjectorPositionStore>();
builder.Services.AddHostedService<ProjectionBackgroundService>();

builder.Services.AddSingleton<SampleDataSeeder>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

// Uncomment to wipe the database and populate it with sample data on every startup.
await app.Services.GetRequiredService<SampleDataSeeder>().ResetAndSeedAsync();

app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
