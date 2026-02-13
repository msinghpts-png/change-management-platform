import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { DatabaseBackup, DatabaseStatus } from "../types/change";

const AdminDatabasePage = () => {
  const [status, setStatus] = useState<DatabaseStatus | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [importJson, setImportJson] = useState("");

  const loadStatus = async () => {
    setError(null);
    try {
      const data = await apiClient.getDatabaseStatus();
      setStatus(data);
    } catch (e) {
      setError((e as Error).message);
    }
  };

  useEffect(() => {
    loadStatus().catch(() => void 0);
  }, []);

  const runAction = async (fn: () => Promise<unknown>) => {
    setLoading(true);
    setError(null);
    try {
      await fn();
      await loadStatus();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const onExport = async () => {
    setLoading(true);
    setError(null);
    try {
      const blob = await apiClient.exportDatabase();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `change-management-backup-${new Date().toISOString()}.json`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const onImport = async () => {
    if (!importJson.trim()) return;
    const payload = JSON.parse(importJson) as DatabaseBackup;
    await runAction(() => apiClient.importDatabase(payload));
  };

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
        <button className="btn" disabled={loading} onClick={() => runAction(() => apiClient.runMigrations())}>Run Migrations</button>
        <button className="btn" disabled={loading} onClick={() => runAction(() => apiClient.seedDatabase())}>Seed Sample Data</button>
        <button className="btn" disabled={loading} onClick={onExport}>Export DB (JSON backup)</button>
        <button className="btn" disabled={loading} onClick={onImport}>Import DB (JSON restore)</button>
      </div>

      <div style={{ height: 12 }} />

      <div className="card card-pad">
        <div className="h3">Import JSON payload</div>
        <textarea
          className="textarea"
          style={{ marginTop: 8, minHeight: 220 }}
          value={importJson}
          onChange={(e) => setImportJson(e.target.value)}
          placeholder='Paste export JSON payload here then click "Import DB".'
        />
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
