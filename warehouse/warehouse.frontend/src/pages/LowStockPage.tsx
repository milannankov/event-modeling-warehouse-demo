import { useEffect, useState } from "react";
import { api, type LowStockItem } from "../api/client";

export default function LowStockPage() {
  const [items, setItems] = useState<LowStockItem[]>([]);

  const load = () => api.getLowStock().then(setItems).catch(() => {});

  useEffect(() => { load(); }, []);

  return (
    <div>
      <h1>Low Stock Alerts</h1>
      <button onClick={load} className="refresh-btn">Refresh</button>

      <table>
        <thead>
          <tr><th>EAN</th><th>Name</th><th>EU VAT</th><th>Price</th><th>Remaining Quantity</th></tr>
        </thead>
        <tbody>
          {items.map(item => (
            <tr key={item.ean} className="low-stock">
              <td>{item.ean}</td>
              <td>{item.name}</td>
              <td>{item.euVat}</td>
              <td>{item.price.toFixed(2)}</td>
              <td>{item.quantity}</td>
            </tr>
          ))}
          {items.length === 0 && <tr><td colSpan={5} className="empty">No low stock alerts.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}
