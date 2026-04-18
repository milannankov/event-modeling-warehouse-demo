import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";

export default function CreateProductPage() {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [ean, setEan] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.createProduct({ name, ean });
      navigate("/products");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create product");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>New Product</h1>

      <form onSubmit={handleSubmit} className="form">
        <div className="form-column">
          <label>Name<input value={name} onChange={e => setName(e.target.value)} required /></label>
          <label>EAN<input value={ean} onChange={e => setEan(e.target.value)} required /></label>
          <button type="submit" disabled={loading}>Create</button>
        </div>
        {error && <p className="error">{error}</p>}
      </form>
    </div>
  );
}
