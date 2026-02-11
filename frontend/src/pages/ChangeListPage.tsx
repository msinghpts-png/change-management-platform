import { useEffect, useMemo, useState, type ChangeEvent } from "react";
import { Link } from "react-router-dom";
import StatusBadge from "../components/StatusBadge";
import { sampleChanges } from "../data/sampleData";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest } from "../types/change";
import "./ChangeListPage.css";

const ChangeListPage = () => {
  const [changes, setChanges] = useState<ChangeRequest[]>(sampleChanges);
  const [query, setQuery] = useState("");

  useEffect(() => {
    apiClient.getChanges().then(setChanges).catch(() => setChanges(sampleChanges));
  }, []);

  const filteredChanges = useMemo(() => {
    const normalized = query.trim().toLowerCase();
    if (!normalized) {
      return changes;
    }

    return changes.filter(
      (change) =>
        change.title.toLowerCase().includes(normalized) ||
        change.description.toLowerCase().includes(normalized) ||
        change.id.toLowerCase().includes(normalized)
    );
  }, [changes, query]);

  return (
    <section>
      <div className="page-header">
        <div>
          <h2 className="page-title">Change Requests</h2>
          <p className="page-subtitle">{filteredChanges.length} change requests shown</p>
        </div>
        <Link className="btn btn-primary" to="/changes/new">
          New Change
        </Link>
      </div>

      <div className="change-list-toolbar card">
        <input
          className="input"
          placeholder="Search by title, description, or change number"
          value={query}
          onChange={(event: ChangeEvent<HTMLInputElement>) => setQuery(event.target.value)}
        />
      </div>

      <div className="card change-list-table-wrap">
        <table className="change-list-table">
          <thead>
            <tr>
              <th>Change</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Risk</th>
              <th>Planned Start</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredChanges.map((change) => (
              <tr key={change.id}>
                <td>
                  <p className="change-list-title">{change.title}</p>
                  <p className="change-list-id">{change.id}</p>
                </td>
                <td>
                  <StatusBadge status={change.status} />
                </td>
                <td>{change.priority ?? "-"}</td>
                <td>{change.riskLevel ?? "-"}</td>
                <td>{change.plannedStart ? new Date(change.plannedStart).toLocaleString() : "Not scheduled"}</td>
                <td>
                  <Link className="change-list-link" to={`/changes/${change.id}`}>
                    Open
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
};

export default ChangeListPage;
