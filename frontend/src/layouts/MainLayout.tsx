import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { routes } from "../routes";

type MainLayoutProps = {
  title: string;
  children: ReactNode;
};

const MainLayout = ({ title, children }: MainLayoutProps) => {
  return (
    <div>
      <header>
        <h1>{title}</h1>
        <nav>
          <ul>
            {routes.map((route) => (
              <li key={route.path}>
                <NavLink to={route.path}>{route.label}</NavLink>
              </li>
            ))}
          </ul>
        </nav>
      </header>
      <main>{children}</main>
    </div>
  );
};

export default MainLayout;
