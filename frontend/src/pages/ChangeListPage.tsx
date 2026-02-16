import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest } from "../types/change";

const riskColor = (risk: string) => ({ Low: "pill-green", Medium: "pill-amber", High: "pill-red", Critical: "pill-darkred" }[risk] ?? "");
const statusColor = (status: string) => ({ Draft: "pill", Submitted: "pill-blue", Approved: "pill-green", Rejected: "pill-red", InImplementation: "pill-orange", Completed: "pill-darkgreen", Closed: "pill-darkgreen" }[status] ?? "pill");

const ChangeListPage = () => {
  const nav = useNavigate();
  const [items, setItems] = useState<ChangeRequest[]>([]);

  useEffect(() => {
    apiClient.getChanges().then(setItems).catch(() => setItems([]));
  }, []);

  return (
    <div>
      <div className="page-head">
        <h1 className="page-title">Change Requests</h1>
        <button className="btn btn-primary" onClick={() => nav("/changes/new")}>+ New Change</button>
      </div>

      <div className="grid" style={{ gap: 12 }}>
        {items.map((c) => (
          <div key={c.changeId} className="card card-pad" style={{ cursor: "pointer" }} onClick={() => nav(`/changes/${c.changeId}`)}>
            <div className="row">
              <div>
                <div className="mono">CHG-{String(c.changeNumber).padStart(6, "0")}</div>
                <div className="h3">{c.title}</div>
              </div>
              <div style={{ display: "flex", gap: 8 }}>
                <span className={`pill ${statusColor(c.status)}`}>{c.status}</span>
                <span className={`pill ${riskColor(c.riskLevel)}`}>Risk: {c.riskLevel}</span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ChangeListPage;
