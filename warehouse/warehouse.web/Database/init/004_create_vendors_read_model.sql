CREATE TABLE IF NOT EXISTS vendors (
    stream_id TEXT PRIMARY KEY,
    eu_vat TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL
);
