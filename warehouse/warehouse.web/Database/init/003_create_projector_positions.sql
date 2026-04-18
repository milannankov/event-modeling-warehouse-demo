CREATE TABLE IF NOT EXISTS projector_positions (
    projector_name TEXT PRIMARY KEY,
    last_processed_event_id INTEGER NOT NULL DEFAULT 0,
    updated_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);
