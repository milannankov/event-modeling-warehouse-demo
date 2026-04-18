namespace Warehouse.EventSourcing;

public class AggregateRepository(IEventStore eventStore)
{
    public async Task<T> LoadAsync<T>(string streamId, CancellationToken ct = default)
        where T : Aggregate, new()
    {
        var events = await eventStore.GetEventsByStreamIdAsync(streamId, ct);
        var aggregate = new T();
        aggregate.Load(events);
        return aggregate;
    }

    public async Task SaveAsync(Aggregate aggregate, CancellationToken ct = default)
    {
        var uncommitted = aggregate.UncommittedEvents;
        if (uncommitted.Count == 0)
            return;

        await eventStore.AppendToStreamAsync(aggregate.StreamId, aggregate.StreamSequenceNumber, uncommitted, ct);
        aggregate.ClearUncommittedEvents();
    }
}
