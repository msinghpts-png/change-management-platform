import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { AuditLog } from "../types/change";

const AuditViewerPage = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);

  useEffect(() => {
    apiClient.getAuditLogs().then(setLogs).catch(() => setLogs([]));
  }, []);

  return <div>
    <h1 className="page-title">Audit Viewer</h1>
    <div className="card card-pad">
      {logs.map((l) => <div key={l.auditLogId} style={{ marginBottom: 10 }}>
        <div className="small">{new Date(l.createdAt).toLocaleString()}</div>
        <div className="h3">{l.action}</div>
        <div>{l.details}</div>
      </div>)}
    </div>
  </div>;
};

export default AuditViewerPage;
