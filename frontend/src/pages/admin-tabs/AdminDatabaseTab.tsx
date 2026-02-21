import { useEffect, useState } from "react";
import { apiClient } from "../../services/apiClient";
import type { DatabaseStatus } from "../../types/change";

const AdminDatabaseTab = () => {
  const [status, setStatus] = useState<DatabaseStatus | null>(null);

  useEffect(() => {
    apiClient.getDatabaseStatus().then(setStatus).catch(() => void 0);
  }, []);

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead><tr><th>Metric</th><th>Value</th></tr></thead>
        <tbody>
          <tr><td>DatabaseName</td><td>{status?.databaseName ?? "—"}</td></tr>
          <tr><td>TotalChanges</td><td>{status?.totalChanges ?? 0}</td></tr>
          <tr><td>TotalApprovals</td><td>{status?.totalApprovals ?? 0}</td></tr>
          <tr><td>TotalAttachments</td><td>{status?.totalAttachments ?? 0}</td></tr>
          <tr><td>HasPendingMigrations</td><td>{status?.hasPendingMigrations ? "Yes" : "No"}</td></tr>
          <tr><td>PendingMigrations</td><td>{status?.pendingMigrations?.join(", ") || "—"}</td></tr>
        </tbody>
      </table>
    </div>
  );
};

export default AdminDatabaseTab;
