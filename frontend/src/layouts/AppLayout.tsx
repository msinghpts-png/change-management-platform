import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { routes } from "../routes";
import "./AppLayout.css";

type AppLayoutProps = {
  children: ReactNode;
};

const AppLayout = ({ children }: AppLayoutProps) => {
  const sidebarRoutes = routes.filter((route) => route.path !== "/changes/:id");

  return (
    <div className="app-layout">
      <aside className="app-sidebar">
        <div className="app-brand">Change Management</div>
        <nav className="app-nav">
          {sidebarRoutes.map((route) => (
            <NavLink
              key={route.path}
              to={route.path}
              className={({ isActive }) => `app-nav-link ${isActive ? "active" : ""}`}
            >
              {route.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <div className="app-content-wrap">
        <header className="app-header">
          <h1 className="app-header-title">Change Management Application</h1>
        </header>
        <main className="app-main">{children}</main>
      </div>
    </div>
  );
};

export default AppLayout;
