-- Event Store Schema
CREATE TABLE IF NOT EXISTS events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    stream_id TEXT NOT NULL,
    stream_sequence_number INTEGER NOT NULL,
    type TEXT NOT NULL,
    version INTEGER NOT NULL DEFAULT 1,
    payload TEXT NOT NULL,
    metadata TEXT NOT NULL DEFAULT '{}',
    created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),

    CONSTRAINT uq_stream_sequence UNIQUE (stream_id, stream_sequence_number)
);

CREATE INDEX IF NOT EXISTS idx_events_stream_id ON events (stream_id);
CREATE INDEX IF NOT EXISTS idx_events_type ON events (type);
CREATE INDEX IF NOT EXISTS idx_events_created_at ON events (created_at);
