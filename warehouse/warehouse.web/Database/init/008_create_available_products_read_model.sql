CREATE TABLE IF NOT EXISTS available_products (
    ean TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    available_quantity INTEGER NOT NULL DEFAULT 0
);
