namespace Warehouse.EventSourcing;

public interface IEventStore
{
    Task<IReadOnlyList<Event>> GetEventsByStreamIdAsync(string streamId, CancellationToken ct = default);
    Task AppendToStreamAsync(string streamId, long expectedSequenceNumber, IEnumerable<Event> events, CancellationToken ct = default);
    Task<IReadOnlyList<Event>> GetEventsAfterAsync(long afterEventId, int batchSize, CancellationToken ct = default);
}

public class OptimisticConcurrencyException : Exception
{
    public string StreamId { get; }
    public long ExpectedSequenceNumber { get; }

    public OptimisticConcurrencyException(string streamId, long expectedSequenceNumber)
        : base($"Concurrency conflict on stream '{streamId}'. Expected sequence number {expectedSequenceNumber} but the stream has advanced.")
    {
        StreamId = streamId;
        ExpectedSequenceNumber = expectedSequenceNumber;
    }
}
