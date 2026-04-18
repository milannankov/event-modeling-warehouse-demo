using Warehouse.EventSourcing;

namespace Warehouse.Tests.EventSourcing;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, List<Event>> _streams = new();
    private long _globalId;

    public Task<IReadOnlyList<Event>> GetEventsByStreamIdAsync(string streamId, CancellationToken ct = default)
    {
        IReadOnlyList<Event> events = _streams.TryGetValue(streamId, out var list)
            ? list.ToList()
            : Array.Empty<Event>();
        return Task.FromResult(events);
    }

    public Task AppendToStreamAsync(string streamId, long expectedSequenceNumber, IEnumerable<Event> events, CancellationToken ct = default)
    {
        if (!_streams.TryGetValue(streamId, out var list))
        {
            list = new List<Event>();
            _streams[streamId] = list;
        }

        if (list.Count != expectedSequenceNumber)
            throw new OptimisticConcurrencyException(streamId, expectedSequenceNumber);

        var seq = expectedSequenceNumber;
        foreach (var evt in events)
        {
            seq++;
            var stamped = evt with
            {
                Id = ++_globalId,
                StreamId = streamId,
                StreamSequenceNumber = seq,
            };
            list.Add(stamped);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Event>> GetEventsAfterAsync(long afterEventId, int batchSize, CancellationToken ct = default)
    {
        IReadOnlyList<Event> events = _streams.Values
            .SelectMany(s => s)
            .Where(e => e.Id > afterEventId)
            .OrderBy(e => e.Id)
            .Take(batchSize)
            .ToList();
        return Task.FromResult(events);
    }
}
