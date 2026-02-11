import "./StatusBadge.css";

type StatusBadgeProps = {
  status: string;
};

const statusClassMap: Record<string, string> = {
  Pending: "warning",
  PendingApproval: "warning",
  Approved: "success",
  Rejected: "danger",
  Scheduled: "info",
  Completed: "success",
  InImplementation: "info",
  Failed: "danger",
  Draft: "neutral"
};

const StatusBadge = ({ status }: StatusBadgeProps) => {
  const normalized = statusClassMap[status] ?? "neutral";
  return <span className={`status-badge ${normalized}`}>{status}</span>;
};

export default StatusBadge;
