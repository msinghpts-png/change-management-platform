import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "../auth";
import AdminDatabasePage from "../pages/AdminDatabasePage";
import AdminUsersPage from "../pages/AdminUsersPage";
import CalendarPage from "../pages/CalendarPage";
import ChangeDetailPage from "../pages/ChangeDetailPage";
import ChangeListPage from "../pages/ChangeListPage";
import DashboardPage from "../pages/DashboardPage";
import LoginPage from "../pages/LoginPage";
import TemplatesPage from "../pages/TemplatesPage";

export const routes = [
  { path: "/dashboard", label: "Dashboard" },
  { path: "/changes", label: "Change List" },
  { path: "/changes/new", label: "New Change" },
  { path: "/changes/:id", label: "Change Detail" },
  { path: "/calendar", label: "Calendar" },
  { path: "/templates", label: "Templates" },
  { path: "/admin/users", label: "Users" },
  { path: "/admin/database", label: "DB Admin" }
];

const Protected = ({ children }: { children: JSX.Element }) => {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return children;
};

export const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<Protected><DashboardPage /></Protected>} />
      <Route path="/changes" element={<Protected><ChangeListPage /></Protected>} />
      <Route path="/changes/new" element={<Protected><ChangeDetailPage /></Protected>} />
      <Route path="/changes/:id" element={<Protected><ChangeDetailPage /></Protected>} />
      <Route path="/calendar" element={<Protected><CalendarPage /></Protected>} />
      <Route path="/templates" element={<Protected><TemplatesPage /></Protected>} />
      <Route path="/admin/users" element={<Protected><AdminUsersPage /></Protected>} />
      <Route path="/admin/database" element={<Protected><AdminDatabasePage /></Protected>} />
    </Routes>
  );
};
