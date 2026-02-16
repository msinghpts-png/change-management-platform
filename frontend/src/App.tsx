import { BrowserRouter, useLocation } from "react-router-dom";
import { AuthProvider } from "./auth";
import { ErrorBoundary } from "./components/ErrorBoundary";
import MainLayout from "./layouts/MainLayout";
import { AppRoutes } from "./routes";

const AppShell = () => {
  const location = useLocation();
  const isLogin = location.pathname === "/login";

  if (isLogin) {
    return <AppRoutes />;
  }

  return (
    <MainLayout title="Change Management">
      <AppRoutes />
    </MainLayout>
  );
};

const App = () => {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <BrowserRouter>
          <AppShell />
        </BrowserRouter>
      </AuthProvider>
    </ErrorBoundary>
  );
};

export default App;
