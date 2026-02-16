import { createTheme } from "@mui/material/styles";

export const appTheme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#2563eb"
    },
    secondary: {
      main: "#0ea5e9"
    },
    background: {
      default: "#f3f6fb",
      paper: "#ffffff"
    },
    success: {
      main: "#16a34a"
    },
    warning: {
      main: "#f59e0b"
    },
    error: {
      main: "#ef4444"
    }
  },
  shape: {
    borderRadius: 12
  },
  spacing: 8,
  typography: {
    fontFamily: '"Inter", "Segoe UI", "Roboto", sans-serif',
    h4: {
      fontWeight: 700
    },
    h5: {
      fontWeight: 700
    },
    subtitle1: {
      color: "#475569"
    }
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          border: "1px solid #e2e8f0",
          boxShadow: "0 6px 20px rgba(15, 23, 42, 0.06)"
        }
      }
    },
    MuiButton: {
      defaultProps: {
        disableElevation: true
      }
    }
  }
});
