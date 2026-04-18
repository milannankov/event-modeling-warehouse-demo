namespace Warehouse.EventSourcing;

public class ProjectionBackgroundService(
    IEventStore eventStore,
    IEnumerable<IProjector> projectors,
    ProjectorPositionStore positionStore,
    ILogger<ProjectionBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(50);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Projection background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var projector in projectors)
            {
                try
                {
                    await ProcessProjectorAsync(projector, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Projector {Projector} failed, will retry next cycle", projector.Name);
                }
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessProjectorAsync(IProjector projector, CancellationToken ct)
    {
        var position = await positionStore.GetPositionAsync(projector.Name, ct);
        var events = await eventStore.GetEventsAfterAsync(position, BatchSize, ct);

        foreach (var evt in events)
        {
            if (projector.CanHandle(evt))
            {
                await projector.ProjectAsync(evt, ct);
            }

            var previousPosition = position;
            position = evt.Id;

            if (!await positionStore.SavePositionAsync(projector.Name, previousPosition, position, ct))
            {
                logger.LogWarning("Projector {Projector} position was advanced by another instance, backing off", projector.Name);
                return;
            }
        }

        if (events.Count > 0)
        {
            logger.LogDebug("Projector {Projector} processed {Count} events, now at position {Position}",
                projector.Name, events.Count, events[^1].Id);
        }
    }
}
