using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Warehouse.Database;

namespace Warehouse.EventSourcing;

public class SqliteEventStore(SqliteConnectionFactory factory) : IEventStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<IReadOnlyList<Event>> GetEventsByStreamIdAsync(string streamId, CancellationToken ct = default)
    {
        var events = new List<Event>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT id, stream_id, stream_sequence_number, type, version, payload, metadata, created_at FROM events WHERE stream_id = @streamId ORDER BY stream_sequence_number",
            conn);
        cmd.Parameters.AddWithValue("streamId", streamId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var evt = ReadEvent(reader);
            if (evt is not null)
                events.Add(evt);
        }

        return events;
    }

    public async Task AppendToStreamAsync(string streamId, long expectedSequenceNumber, IEnumerable<Event> events, CancellationToken ct = default)
    {
        var eventList = events.ToList();

        await using var conn = await factory.OpenAsync(ct);
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);

        try
        {
            var sequenceNumber = expectedSequenceNumber;
            foreach (var evt in eventList)
            {
                sequenceNumber++;
                var payload = JsonSerializer.Serialize(evt, evt.GetType(), JsonOptions);
                var metadata = JsonSerializer.Serialize(evt.Metadata, JsonOptions);

                await using var cmd = new SqliteCommand(
                    "INSERT INTO events (stream_id, stream_sequence_number, type, version, payload, metadata) VALUES (@streamId, @sequenceNumber, @type, @version, @payload, @metadata)",
                    conn, tx);
                cmd.Parameters.AddWithValue("streamId", streamId);
                cmd.Parameters.AddWithValue("sequenceNumber", sequenceNumber);
                cmd.Parameters.AddWithValue("type", evt.Type);
                cmd.Parameters.AddWithValue("version", evt.Version);
                cmd.Parameters.AddWithValue("payload", payload);
                cmd.Parameters.AddWithValue("metadata", metadata);

                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            await tx.RollbackAsync(ct);
            throw new OptimisticConcurrencyException(streamId, expectedSequenceNumber);
        }
    }

    public async Task<IReadOnlyList<Event>> GetEventsAfterAsync(long afterEventId, int batchSize, CancellationToken ct = default)
    {
        var events = new List<Event>();

        await using var conn = await factory.OpenAsync(ct);
        await using var cmd = new SqliteCommand(
            "SELECT id, stream_id, stream_sequence_number, type, version, payload, metadata, created_at FROM events WHERE id > @afterEventId ORDER BY id LIMIT @batchSize",
            conn);
        cmd.Parameters.AddWithValue("afterEventId", afterEventId);
        cmd.Parameters.AddWithValue("batchSize", batchSize);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var evt = ReadEvent(reader);
            if (evt is not null)
                events.Add(evt);
        }

        return events;
    }

    private static Event? ReadEvent(SqliteDataReader reader)
    {
        var typeName = reader.GetString(3);
        var eventType = EventTypeRegistry.Resolve(typeName);
        if (eventType is null)
            return null;

        var payload = reader.GetString(5);
        if (JsonSerializer.Deserialize(payload, eventType, JsonOptions) is not Event evt)
            return null;

        var metadataJson = reader.GetString(6);
        var metadata = JsonSerializer.Deserialize<EventMetadata>(metadataJson, JsonOptions) ?? new EventMetadata();

        var createdAtText = reader.GetString(7);
        var createdAt = DateTimeOffset.Parse(createdAtText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

        return evt with
        {
            Id = reader.GetInt64(0),
            StreamId = reader.GetString(1),
            StreamSequenceNumber = reader.GetInt64(2),
            Version = reader.GetInt32(4),
            CreatedAt = createdAt,
            Metadata = metadata
        };
    }
}
