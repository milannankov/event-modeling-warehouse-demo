import { useEffect, useState, type FormEvent } from "react";
import { api, type Product, type Vendor } from "../api/client";

export default function PurchasesPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [ean, setEan] = useState("");
  const [euVat, setEuVat] = useState("");
  const [price, setPrice] = useState("");
  const [quantity, setQuantity] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.getProducts().then(setProducts).catch(() => {});
    api.getVendors().then(setVendors).catch(() => {});
  }, []);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      await api.createPurchase({
        ean,
        euVat,
        price: parseFloat(price),
        quantity: parseInt(quantity),
      });
      setPrice("");
      setQuantity("");
      setSuccess("Purchase created successfully.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create purchase");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>Wholesale Purchase</h1>

      <form onSubmit={handleSubmit} className="form">
        <h3>New Purchase</h3>
        <div className="form-column">
          <label>
            Product
            <select value={ean} onChange={e => setEan(e.target.value)} required>
              <option value="">Select product...</option>
              {products.map(p => (
                <option key={p.ean} value={p.ean}>{p.name} ({p.ean})</option>
              ))}
            </select>
          </label>
          <label>
            Vendor
            <select value={euVat} onChange={e => setEuVat(e.target.value)} required>
              <option value="">Select vendor...</option>
              {vendors.map(v => (
                <option key={v.euVat} value={v.euVat}>{v.name} ({v.euVat})</option>
              ))}
            </select>
          </label>
          <label>Price<input type="number" step="0.01" value={price} onChange={e => setPrice(e.target.value)} required /></label>
          <label>Quantity<input type="number" min="1" value={quantity} onChange={e => setQuantity(e.target.value)} required /></label>
          <button type="submit" disabled={loading}>Create Purchase</button>
        </div>
        {error && <p className="error">{error}</p>}
        {success && <p className="success">{success}</p>}
      </form>
    </div>
  );
}
