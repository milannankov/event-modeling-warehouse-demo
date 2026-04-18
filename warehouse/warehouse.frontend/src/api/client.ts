async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }
  if (res.status === 201 || res.headers.get("content-length") === "0") return undefined as T;
  return res.json();
}

export interface Product {
  streamId: string;
  name: string;
  ean: string;
}

export interface Vendor {
  streamId: string;
  euVat: string;
  name: string;
}

export interface AvailableProduct {
  ean: string;
  name: string;
  availableQuantity: number;
}

export interface InventoryItem {
  ean: string;
  name: string;
  quantity: number;
}

export interface LowStockItem {
  ean: string;
  name: string;
  euVat: string;
  price: number;
  quantity: number;
}

export const api = {
  getProducts: () => request<Product[]>("/api/products"),
  createProduct: (data: { name: string; ean: string }) =>
    request<void>("/api/products", { method: "POST", body: JSON.stringify(data) }),

  getVendors: () => request<Vendor[]>("/api/vendors"),
  createVendor: (data: { euVat: string; name: string }) =>
    request<void>("/api/vendors", { method: "POST", body: JSON.stringify(data) }),

  createPurchase: (data: { ean: string; euVat: string; price: number; quantity: number }) =>
    request<void>("/api/purchases", { method: "POST", body: JSON.stringify(data) }),

  getAvailableProducts: () => request<AvailableProduct[]>("/api/sales/products"),
  createSale: (data: { ean: string; clientName: string; salePrice: number; quantity: number }) =>
    request<void>("/api/sales", { method: "POST", body: JSON.stringify(data) }),

  getInventory: () => request<InventoryItem[]>("/api/inventory"),

  getLowStock: () => request<LowStockItem[]>("/api/low-stock"),
};
