import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Layout from "./components/Layout";
import ProductsPage from "./pages/ProductsPage";
import CreateProductPage from "./pages/CreateProductPage";
import VendorsPage from "./pages/VendorsPage";
import CreateVendorPage from "./pages/CreateVendorPage";
import PurchasesPage from "./pages/PurchasesPage";
import SalesPage from "./pages/SalesPage";
import InventoryPage from "./pages/InventoryPage";
import LowStockPage from "./pages/LowStockPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Navigate to="/products" replace />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/products/new" element={<CreateProductPage />} />
          <Route path="/vendors" element={<VendorsPage />} />
          <Route path="/vendors/new" element={<CreateVendorPage />} />
          <Route path="/purchases" element={<PurchasesPage />} />
          <Route path="/sales" element={<SalesPage />} />
          <Route path="/inventory" element={<InventoryPage />} />
          <Route path="/low-stock" element={<LowStockPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
