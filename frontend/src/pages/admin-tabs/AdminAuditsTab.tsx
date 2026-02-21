import { useEffect, useState } from "react";
import { apiClient } from "../../services/apiClient";
import type { AdminAuditEvent } from "../../types/change";

const formatDate = (value?: string) => (value ? new Date(value).toLocaleString() : "—");

const AdminAuditsTab = () => {
  const [audits, setAudits] = useState<AdminAuditEvent[]>([]);

  useEffect(() => {
    apiClient.getAuditEvents().then(setAudits).catch(() => void 0);
  }, []);

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead><tr><th>AuditEventId</th><th>EventTypeId</th><th>ActorUserId</th><th>ActorUpn</th><th>SchemaName</th><th>EntityName</th><th>EntityId</th><th>EntityNumber</th><th>CreatedAt</th></tr></thead>
        <tbody>
          {audits.map((audit) => (
            <tr key={audit.auditEventId}>
              <td>{audit.auditEventId}</td>
              <td>{audit.eventType ? `${audit.eventTypeId} (${audit.eventType})` : audit.eventTypeId}</td>
              <td>{audit.actorUserId || "—"}</td>
              <td>{audit.actorUpn || "—"}</td>
              <td>{audit.schemaName || "—"}</td>
              <td>{audit.entityName || "—"}</td>
              <td>{audit.entityId || "—"}</td>
              <td>{audit.entityNumber || "—"}</td>
              <td>{formatDate(audit.createdAt)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AdminAuditsTab;
