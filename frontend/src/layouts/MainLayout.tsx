import type { ReactNode } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth";
import { routes } from "../routes";

type MainLayoutProps = {
  title: string;
  children: ReactNode;
};

const MainLayout = ({ title, children }: MainLayoutProps) => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const current = routes.find((r) => location.pathname.startsWith(r.path)) ?? routes[0];

  return (
    <div>
      <div className="topbar">
        <div className="topbar-inner">
          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <select className="select" value={current.path} onChange={(e) => navigate(e.target.value)} aria-label="Navigate">
              {routes.map((r) => (
                <option key={r.path} value={r.path}>{r.label}</option>
              ))}
            </select>
            <button className="iconbtn" type="button" title="Refresh" onClick={() => window.location.reload()}>↻</button>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <span className="small">{user?.upn}</span>
            <button className="btn" onClick={() => { logout(); navigate('/login'); }}>Logout</button>
            <button className="btn btn-primary" type="button" onClick={() => navigate("/changes/new")}>New Change Request</button>
          </div>
        </div>
      </div>

      <main className="container" aria-label={title}>{children}</main>
    </div>
  );
};

export default MainLayout;
