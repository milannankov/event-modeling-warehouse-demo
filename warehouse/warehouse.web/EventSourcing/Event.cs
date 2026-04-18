using System.Text.Json;

namespace Warehouse.EventSourcing;

public abstract record Event
{
    public long Id { get; init; }
    public string StreamId { get; init; } = default!;
    public long StreamSequenceNumber { get; init; }
    public string Type => GetType().Name;
    public int Version { get; init; } = 1;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public EventMetadata Metadata { get; init; } = new();
}

public record EventMetadata
{
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public string? UserId { get; init; }
    public string? Source { get; init; }
}
