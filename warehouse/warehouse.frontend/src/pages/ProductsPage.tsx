import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api, type Product } from "../api/client";

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);

  useEffect(() => {
    api.getProducts().then(setProducts).catch(() => {});
  }, []);

  return (
    <div>
      <h1>Products</h1>

      <Link to="/products/new" className="new-btn">+ New Product</Link>

      <table>
        <thead>
          <tr><th>Name</th><th>EAN</th></tr>
        </thead>
        <tbody>
          {products.map(p => (
            <tr key={p.ean}><td>{p.name}</td><td>{p.ean}</td></tr>
          ))}
          {products.length === 0 && <tr><td colSpan={2} className="empty">No products yet.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}
