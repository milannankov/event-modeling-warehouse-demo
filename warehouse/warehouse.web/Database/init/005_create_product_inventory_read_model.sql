CREATE TABLE IF NOT EXISTS product_inventory (
    ean TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 0
);
