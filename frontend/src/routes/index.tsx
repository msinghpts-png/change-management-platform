import { Navigate, Route, Routes } from "react-router-dom";
import CalendarPage from "../pages/CalendarPage";
import ChangeDetailPage from "../pages/ChangeDetailPage";
import ChangeListPage from "../pages/ChangeListPage";
import DashboardPage from "../pages/DashboardPage";
import TemplatesPage from "../pages/TemplatesPage";

export const routes = [
  { path: "/dashboard", label: "Dashboard" },
  { path: "/changes", label: "Change List" },
  { path: "/changes/new", label: "New Change" },
  { path: "/changes/:id", label: "Change Detail" },
  { path: "/calendar", label: "Calendar" },
  { path: "/templates", label: "Templates" }
];

export const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<DashboardPage />} />
      <Route path="/changes" element={<ChangeListPage />} />
      <Route path="/changes/new" element={<ChangeDetailPage />} />
      <Route path="/changes/:id" element={<ChangeDetailPage />} />
      <Route path="/calendar" element={<CalendarPage />} />
      <Route path="/templates" element={<TemplatesPage />} />
    </Routes>
  );
};
