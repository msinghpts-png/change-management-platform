import { createContext, useContext, useMemo, useState } from "react";
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
    const raw = localStorage.getItem("authUser");
    return raw ? (JSON.parse(raw) as AppUser) : null;
  });

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
