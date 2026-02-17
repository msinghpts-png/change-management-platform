import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { DatabaseStatus } from "../types/change";

const AdminDatabasePage = () => {
  const [status, setStatus] = useState<DatabaseStatus | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadStatus = async () => {
    setError(null);
    try {
      const data = await apiClient.getDatabaseStatus();
      setStatus(data);
    } catch (e) {
      setError((e as Error).message);
    }
  };

  const loadDemoData = async () => {
    setError(null);
    try {
      await apiClient.seedDatabase();
      await loadStatus();
    } catch (e) {
      setError((e as Error).message);
    }
  };

  useEffect(() => {
    loadStatus().catch(() => void 0);
  }, []);

  return (
    <div>
      <div className="page-head">
        <div>
          <h1 className="page-title">Admin Database Management</h1>
          <p className="page-subtitle">Inspect and manage SQL persistence and migration state.</p>
        </div>
      </div>

      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}

      <div className="card card-pad">
        <div className="grid grid-2">
          <div>
            <div className="small">Database Name</div>
            <div className="h3">{status?.databaseName ?? "—"}</div>
          </div>
          <div>
            <div className="small">Migration Status</div>
            <div className="h3">{status?.hasPendingMigrations ? "Pending" : "Up to date"}</div>
          </div>
          <div>
            <div className="small">Total Changes</div>
            <div className="h3">{status?.totalChanges ?? 0}</div>
          </div>
          <div>
            <div className="small">Total Approvals</div>
            <div className="h3">{status?.totalApprovals ?? 0}</div>
          </div>
        </div>
      </div>

      <div style={{ height: 12 }} />

      <div className="card card-pad" style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
        <button className="btn" onClick={loadStatus}>Refresh</button>
        <button className="btn btn-primary" onClick={loadDemoData}>Load Demo Data</button>
        <div className="small" style={{ alignSelf: "center" }}>Read-only maintenance view (no destructive actions).</div>
      </div>

      {status?.pendingMigrations?.length ? (
        <div className="card card-pad" style={{ marginTop: 12 }}>
          <div className="h3">Pending migrations</div>
          <ul>
            {status.pendingMigrations.map((item) => <li key={item}>{item}</li>)}
          </ul>
        </div>
      ) : null}
    </div>
  );
};

export default AdminDatabasePage;
