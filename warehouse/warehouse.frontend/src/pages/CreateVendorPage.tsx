import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";

export default function CreateVendorPage() {
  const navigate = useNavigate();
  const [euVat, setEuVat] = useState("");
  const [name, setName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.createVendor({ euVat, name });
      navigate("/vendors");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create vendor");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>New Vendor</h1>

      <form onSubmit={handleSubmit} className="form">
        <div className="form-column">
          <label>EU VAT<input value={euVat} onChange={e => setEuVat(e.target.value)} required /></label>
          <label>Name<input value={name} onChange={e => setName(e.target.value)} required /></label>
          <button type="submit" disabled={loading}>Create</button>
        </div>
        {error && <p className="error">{error}</p>}
      </form>
    </div>
  );
}
