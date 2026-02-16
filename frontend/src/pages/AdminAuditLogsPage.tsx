import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";

const AdminAuditLogsPage = () => {
  const [items, setItems] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiClient.getAuditEvents().then(setItems).catch((e) => setError((e as Error).message));
  }, []);

  return (
    <div>
      <div className="page-head"><h1 className="page-title">Audit Logs</h1></div>
      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}
      <div className="card card-pad">
        <table style={{ width: "100%" }}>
          <thead><tr><th>When</th><th>Actor</th><th>Entity</th><th>Reason</th><th>Details</th></tr></thead>
          <tbody>
            {items.map((x) => (
              <tr key={x.auditEventId}>
                <td>{x.eventAt}</td>
                <td>{x.actorUpn}</td>
                <td>{x.entityName}</td>
                <td>{x.reason}</td>
                <td>{x.details}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminAuditLogsPage;
