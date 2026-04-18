import { useEffect, useState } from "react";
import { api, type InventoryItem } from "../api/client";

export default function InventoryPage() {
  const [items, setItems] = useState<InventoryItem[]>([]);

  const load = () => api.getInventory().then(setItems).catch(() => {});

  useEffect(() => { load(); }, []);

  return (
    <div>
      <h1>Product Inventory</h1>
      <button onClick={load} className="refresh-btn">Refresh</button>

      <table>
        <thead>
          <tr><th>EAN</th><th>Name</th><th>Quantity</th></tr>
        </thead>
        <tbody>
          {items.map(item => (
            <tr key={item.ean}>
              <td>{item.ean}</td>
              <td>{item.name}</td>
              <td>{item.quantity}</td>
            </tr>
          ))}
          {items.length === 0 && <tr><td colSpan={3} className="empty">No inventory data yet.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}
