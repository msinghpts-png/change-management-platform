import { useEffect, useMemo, useState } from "react";
import { apiClient } from "../services/apiClient";

const fmt = (value?: string) => {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString(undefined, {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  });
};

const AdminAuditLogsPage = () => {
  const [items, setItems] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [entityFilter, setEntityFilter] = useState("");
  const [actorFilter, setActorFilter] = useState("");

  useEffect(() => {
    apiClient.getAuditEvents().then(setItems).catch((e) => setError((e as Error).message));
  }, []);

  const filtered = useMemo(() => {
    const entity = entityFilter.trim().toLowerCase();
    const actor = actorFilter.trim().toLowerCase();
    return items.filter((x) => (entity ? (x.entityName ?? "").toLowerCase().includes(entity) : true))
      .filter((x) => (actor ? (x.actorUpn ?? "").toLowerCase().includes(actor) : true));
  }, [items, entityFilter, actorFilter]);

  return (
    <div>
      <div className="page-head"><h1 className="page-title">Audit Logs</h1></div>
      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}
      <div className="card card-pad" style={{ display: "flex", gap: 8, marginBottom: 12 }}>
        <input className="input" placeholder="Filter by entity" value={entityFilter} onChange={(e) => setEntityFilter(e.target.value)} />
        <input className="input" placeholder="Filter by actor" value={actorFilter} onChange={(e) => setActorFilter(e.target.value)} />
      </div>
      <div className="card card-pad" style={{ overflow: "auto", maxHeight: 520 }}>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead style={{ position: "sticky", top: 0, background: "#e2e8f0" }}>
            <tr>
              <th style={{ textAlign: "left", padding: 8, fontWeight: 800 }}>When</th>
              <th style={{ textAlign: "left", padding: 8, fontWeight: 800 }}>Actor</th>
              <th style={{ textAlign: "left", padding: 8, fontWeight: 800 }}>Entity</th>
              <th style={{ textAlign: "left", padding: 8, fontWeight: 800 }}>Change</th>
              <th style={{ textAlign: "left", padding: 8, fontWeight: 800 }}>Reason</th>
              <th style={{ textAlign: "left", padding: 8, fontWeight: 800 }}>Details</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((x, idx) => (
              <tr key={x.auditEventId} style={{ background: idx % 2 === 0 ? "#f8fafc" : "transparent" }}>
                <td style={{ padding: 8 }}>{fmt(x.eventAt)}</td>
                <td style={{ padding: 8 }}>{x.actorUpn}</td>
                <td style={{ padding: 8 }}>{x.entityName}</td>
                <td style={{ padding: 8 }}>{x.changeNumber || "—"}</td>
                <td style={{ padding: 8 }}>{x.reason}</td>
                <td style={{ padding: 8 }}>{x.details}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminAuditLogsPage;
