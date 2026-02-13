import type { ReactNode } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { routes } from "../routes";

type MainLayoutProps = {
  title: string;
  children: ReactNode;
};

const IconRefresh = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
    <path
      d="M21 12a9 9 0 1 1-2.64-6.36"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
    />
    <path
      d="M21 3v7h-7"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const IconPlus = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
    <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
  </svg>
);

const MainLayout = ({ title, children }: MainLayoutProps) => {
  const location = useLocation();
  const navigate = useNavigate();

  const current = routes.find((r) => location.pathname.startsWith(r.path)) ?? routes[0];

  return (
    <div>
      <div className="topbar">
        <div className="topbar-inner">
          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <select
              className="select"
              value={current.path}
              onChange={(e) => navigate(e.target.value)}
              aria-label="Navigate"
            >
              {routes.map((r) => (
                <option key={r.path} value={r.path}>
                  {r.label}
                </option>
              ))}
            </select>

            <button className="iconbtn" type="button" title="Refresh" onClick={() => window.location.reload()}>
              <IconRefresh />
            </button>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <button className="btn btn-primary" type="button" onClick={() => navigate("/changes/new")}>
              <IconPlus />
              New Change Request
            </button>
          </div>
        </div>
      </div>

      <main className="container" aria-label={title}>
        {children}
      </main>
    </div>
  );
};

export default MainLayout;
