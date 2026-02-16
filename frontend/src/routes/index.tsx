import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "../auth";
import AdminUsersPage from "../pages/AdminUsersPage";
import AuditViewerPage from "../pages/AuditViewerPage";
import AttachmentManagementPage from "../pages/AttachmentManagementPage";
import ChangeDetailPage from "../pages/ChangeDetailPage";
import ChangeListPage from "../pages/ChangeListPage";
import LoginPage from "../pages/LoginPage";

export const routes = [
  { path: "/changes", label: "Change List" },
  { path: "/changes/new", label: "New Change" },
  { path: "/admin/users", label: "User Maintenance" },
  { path: "/admin/audit", label: "Audit Viewer" },
  { path: "/admin/attachments", label: "Attachment Management" }
];

const Protected = ({ children }: { children: JSX.Element }) => {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return children;
};

export const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<Navigate to="/changes" replace />} />
    <Route path="/login" element={<LoginPage />} />
    <Route path="/changes" element={<Protected><ChangeListPage /></Protected>} />
    <Route path="/changes/new" element={<Protected><ChangeDetailPage /></Protected>} />
    <Route path="/changes/:id" element={<Protected><ChangeDetailPage /></Protected>} />
    <Route path="/admin/users" element={<Protected><AdminUsersPage /></Protected>} />
    <Route path="/admin/audit" element={<Protected><AuditViewerPage /></Protected>} />
    <Route path="/admin/attachments" element={<Protected><AttachmentManagementPage /></Protected>} />
  </Routes>
);
