import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "../auth";
import AdminPage from "../pages/AdminPage";
import CalendarPage from "../pages/CalendarPage";
import ChangeDetailPage from "../pages/ChangeDetailPage";
import ChangeListPage from "../pages/ChangeListPage";
import DashboardPage from "../pages/DashboardPage";
import LoginPage from "../pages/LoginPage";

export const rootNavRoutes = [
  { path: "/dashboard", label: "Dashboard" },
  { path: "/changes", label: "Change List" },
  { path: "/changes/new", label: "New Change" },
  { path: "/calendar", label: "Calendar" }
];

export const getNavigationRoutes = (role?: string) => {
  const routes = [...rootNavRoutes];
  if (role === "Admin") {
    routes.push({ path: "/admin", label: "Admin" });
  }

  return routes;
};

const Protected = ({ children }: { children: JSX.Element }) => {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return children;
};

const AdminProtected = ({ children }: { children: JSX.Element }) => {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (user.role !== "Admin") return <Navigate to="/dashboard" replace />;
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
      <Route path="/admin" element={<AdminProtected><AdminPage /></AdminProtected>} />
    </Routes>
  );
};
