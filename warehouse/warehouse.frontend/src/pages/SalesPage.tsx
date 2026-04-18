import { useEffect, useState, type FormEvent } from "react";
import { api, type AvailableProduct } from "../api/client";

export default function SalesPage() {
  const [products, setProducts] = useState<AvailableProduct[]>([]);
  const [ean, setEan] = useState("");
  const [clientName, setClientName] = useState("");
  const [salePrice, setSalePrice] = useState("");
  const [quantity, setQuantity] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.getAvailableProducts().then(setProducts).catch(() => {});
  }, []);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      await api.createSale({
        ean,
        clientName,
        salePrice: parseFloat(salePrice),
        quantity: parseInt(quantity),
      });
      setClientName("");
      setSalePrice("");
      setQuantity("");
      setSuccess("Sale recorded successfully.");
      api.getAvailableProducts().then(setProducts).catch(() => {});
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to record sale");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>Product Sale</h1>

      <form onSubmit={handleSubmit} className="form">
        <h3>New Sale</h3>
        <div className="form-column">
          <label>
            Product
            <select value={ean} onChange={e => setEan(e.target.value)} required>
              <option value="">Select product...</option>
              {products.map(p => (
                <option key={p.ean} value={p.ean}>{p.name} ({p.ean}) — available: {p.availableQuantity}</option>
              ))}
            </select>
          </label>
          <label>Client Name<input value={clientName} onChange={e => setClientName(e.target.value)} required /></label>
          <label>Sale Price<input type="number" step="0.01" value={salePrice} onChange={e => setSalePrice(e.target.value)} required /></label>
          <label>Quantity<input type="number" min="1" value={quantity} onChange={e => setQuantity(e.target.value)} required /></label>
          <button type="submit" disabled={loading}>Record Sale</button>
        </div>
        {error && <p className="error">{error}</p>}
        {success && <p className="success">{success}</p>}
      </form>
    </div>
  );
}
