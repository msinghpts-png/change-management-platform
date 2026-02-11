export const appTheme = {
  colors: {
    background: "#f4f6fb",
    surface: "#ffffff",
    surfaceMuted: "#f8fafc",
    border: "#d9e0ea",
    textPrimary: "#0f172a",
    textSecondary: "#475569",
    primary: "#2563eb",
    primaryDark: "#1d4ed8",
    success: "#16a34a",
    warning: "#f59e0b",
    danger: "#ef4444",
    info: "#0ea5e9"
  },
  spacing: {
    xs: "4px",
    sm: "8px",
    md: "12px",
    lg: "16px",
    xl: "24px",
    xxl: "32px"
  },
  radius: {
    sm: "6px",
    md: "10px",
    lg: "14px"
  },
  typography: {
    fontFamily: '"Inter", "Segoe UI", "Roboto", sans-serif',
    title: "28px",
    section: "20px",
    body: "14px",
    small: "12px"
  }
};

export const applyThemeVariables = () => {
  const root = document.documentElement;
  root.style.setProperty("--color-background", appTheme.colors.background);
  root.style.setProperty("--color-surface", appTheme.colors.surface);
  root.style.setProperty("--color-surface-muted", appTheme.colors.surfaceMuted);
  root.style.setProperty("--color-border", appTheme.colors.border);
  root.style.setProperty("--color-text-primary", appTheme.colors.textPrimary);
  root.style.setProperty("--color-text-secondary", appTheme.colors.textSecondary);
  root.style.setProperty("--color-primary", appTheme.colors.primary);
  root.style.setProperty("--color-primary-dark", appTheme.colors.primaryDark);
  root.style.setProperty("--color-success", appTheme.colors.success);
  root.style.setProperty("--color-warning", appTheme.colors.warning);
  root.style.setProperty("--color-danger", appTheme.colors.danger);
  root.style.setProperty("--color-info", appTheme.colors.info);

  root.style.setProperty("--space-xs", appTheme.spacing.xs);
  root.style.setProperty("--space-sm", appTheme.spacing.sm);
  root.style.setProperty("--space-md", appTheme.spacing.md);
  root.style.setProperty("--space-lg", appTheme.spacing.lg);
  root.style.setProperty("--space-xl", appTheme.spacing.xl);
  root.style.setProperty("--space-xxl", appTheme.spacing.xxl);

  root.style.setProperty("--radius-sm", appTheme.radius.sm);
  root.style.setProperty("--radius-md", appTheme.radius.md);
  root.style.setProperty("--radius-lg", appTheme.radius.lg);

  root.style.setProperty("--font-family", appTheme.typography.fontFamily);
  root.style.setProperty("--font-title", appTheme.typography.title);
  root.style.setProperty("--font-section", appTheme.typography.section);
  root.style.setProperty("--font-body", appTheme.typography.body);
  root.style.setProperty("--font-small", appTheme.typography.small);
};
