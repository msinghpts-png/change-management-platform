import { BrowserRouter } from "react-router-dom";
import AppLayout from "./layouts/AppLayout";
import { AppRoutes } from "./routes";

const App = () => {
  return (
    <BrowserRouter>
      <AppLayout>
        <AppRoutes />
      </AppLayout>
    </BrowserRouter>
  );
};

export default App;
