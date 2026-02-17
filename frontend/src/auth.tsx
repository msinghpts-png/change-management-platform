import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { apiClient } from "./services/apiClient";
import type { AppUser } from "./types/change";

type AuthContextValue = {
  user: AppUser | null;
  login: (upn: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<AppUser | null>(() => {
    const rawUser = localStorage.getItem("authUser");
    const token = localStorage.getItem("authToken");
    if (!rawUser || !token) return null;
    return JSON.parse(rawUser) as AppUser;
  });


  useEffect(() => {
    if (!(import.meta as any).env?.DEV) return;
    const token = localStorage.getItem("authToken");
    if (token || user) return;

    apiClient.login("admin@local", "Admin123!")
      .then((result) => {
        localStorage.setItem("authToken", result.token);
        localStorage.setItem("authUser", JSON.stringify(result.user));
        setUser(result.user);
      })
      .catch(() => void 0);
  }, [user]);

  const login = async (upn: string, password: string) => {
    const result = await apiClient.login(upn, password);
    localStorage.setItem("authToken", result.token);
    localStorage.setItem("authUser", JSON.stringify(result.user));
    setUser(result.user);
  };

  const logout = () => {
    localStorage.removeItem("authToken");
    localStorage.removeItem("authUser");
    setUser(null);
  };

  const value = useMemo(() => ({ user, login, logout }), [user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
};
