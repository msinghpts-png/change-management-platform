import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { routes } from "../routes";
import "./layout.css";

type MainLayoutProps = {
  title: string;
  children: ReactNode;
};

const MainLayout = ({ title, children }: MainLayoutProps) => {
  return (
    <div className="app-container">
      <aside className="sidebar">
        <h1 className="logo">{title}</h1>
        <nav>
          {routes.map((route) => (
            <NavLink
              key={route.path}
              to={route.path}
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              {route.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="content">
        {children}
      </div>
    </div>
  );
};

export default MainLayout;
