CREATE TABLE IF NOT EXISTS inventory_levels (
    ean TEXT PRIMARY KEY,
    name TEXT NOT NULL DEFAULT '',
    quantity INTEGER NOT NULL DEFAULT 0,
    has_purchased INTEGER NOT NULL DEFAULT 0
);
