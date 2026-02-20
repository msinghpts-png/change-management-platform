import type { CSSProperties } from "react";

type PriorityBadgeProps = {
  priority?: string;
};

const normalizePriority = (priority?: string) => {
  const value = (priority ?? "P3").trim().toUpperCase();
  if (["P1", "P2", "P3", "P4"].includes(value)) {
    return value;
  }

  if (value === "CRITICAL") return "P1";
  if (value === "HIGH") return "P2";
  if (value === "MEDIUM") return "P3";
  if (value === "LOW") return "P4";

  return "P3";
};

const stylesByPriority: Record<string, CSSProperties> = {
  P1: { backgroundColor: "#7f1d1d", borderColor: "#dc2626", color: "#fee2e2" },
  P2: { backgroundColor: "#7c2d12", borderColor: "#f97316", color: "#ffedd5" },
  P3: { backgroundColor: "#1e3a8a", borderColor: "#3b82f6", color: "#dbeafe" },
  P4: { backgroundColor: "#374151", borderColor: "#9ca3af", color: "#f3f4f6" }
};

const PriorityBadge = ({ priority }: PriorityBadgeProps) => {
  const normalized = normalizePriority(priority);
  const style = stylesByPriority[normalized];

  return (
    <span className="pill" style={style}>
      {normalized}
    </span>
  );
};

export default PriorityBadge;
