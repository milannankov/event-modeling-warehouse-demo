import { NavLink, Outlet } from "react-router-dom";

export default function Layout() {
  return (
    <div className="app">
      <nav className="sidebar">
        <h2>Warehouse</h2>
        <ul>
          <li><NavLink to="/products">Products</NavLink></li>
          <li><NavLink to="/vendors">Vendors</NavLink></li>
          <li><NavLink to="/purchases">Wholesale Purchase</NavLink></li>
          <li><NavLink to="/sales">Product Sale</NavLink></li>
          <li><NavLink to="/inventory">Product Inventory</NavLink></li>
          <li><NavLink to="/low-stock">Low Stock Alerts</NavLink></li>
        </ul>
      </nav>
      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}
