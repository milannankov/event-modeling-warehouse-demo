import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api, type Vendor } from "../api/client";

export default function VendorsPage() {
  const [vendors, setVendors] = useState<Vendor[]>([]);

  useEffect(() => {
    api.getVendors().then(setVendors).catch(() => {});
  }, []);

  return (
    <div>
      <h1>Vendors</h1>

      <Link to="/vendors/new" className="new-btn">+ New Vendor</Link>

      <table>
        <thead>
          <tr><th>EU VAT</th><th>Name</th></tr>
        </thead>
        <tbody>
          {vendors.map(v => (
            <tr key={v.euVat}><td>{v.euVat}</td><td>{v.name}</td></tr>
          ))}
          {vendors.length === 0 && <tr><td colSpan={2} className="empty">No vendors yet.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}
