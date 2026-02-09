import { BrowserRouter } from "react-router-dom";
import MainLayout from "./layouts/MainLayout";
import { AppRoutes } from "./routes";

const App = () => {
  return (
    <BrowserRouter>
      <MainLayout title="Change Management">
        <AppRoutes />
      </MainLayout>
    </BrowserRouter>
  );
};

export default App;
