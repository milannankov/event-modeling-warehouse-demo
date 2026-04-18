using Microsoft.Data.Sqlite;
using Warehouse.EventSourcing;
using Warehouse.Database;

namespace Warehouse.Slices.LowStockAlert;

public class LowStockAlertProcessor(
    SqliteConnectionFactory factory,
    RaiseLowStockAlertCommandHandler commandHandler,
    ILogger<LowStockAlertProcessor> logger) : BackgroundService
{
    private const int LowStockThreshold = 10;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await TickAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Low stock alert processor tick failed");
            }

            try
            {
                await Task.Delay(PollInterval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        var lowStock = new List<(string Ean, string Name, int Quantity)>();

        await using var conn = await factory.OpenAsync(ct);
        await using (var cmd = new SqliteCommand(
            "SELECT ean, name, quantity FROM inventory_levels WHERE quantity < @threshold AND has_purchased = 1",
            conn))
        {
            cmd.Parameters.AddWithValue("threshold", LowStockThreshold);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                lowStock.Add((reader.GetString(0), reader.GetString(1), reader.GetInt32(2)));
            }
        }

        foreach (var row in lowStock)
        {
            await commandHandler.HandleAsync(new RaiseLowStockAlertCommand
            {
                Ean = row.Ean,
                Name = row.Name,
                Quantity = row.Quantity,
            }, ct);
        }
    }
}
