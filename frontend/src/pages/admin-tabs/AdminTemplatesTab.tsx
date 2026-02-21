import { useEffect, useState } from "react";
import { apiClient } from "../../services/apiClient";
import type { AdminTemplate } from "../../types/change";

const formatDate = (value?: string) => (value ? new Date(value).toLocaleString() : "—");

const AdminTemplatesTab = () => {
  const [templates, setTemplates] = useState<AdminTemplate[]>([]);

  useEffect(() => {
    apiClient.getAdminTemplates().then(setTemplates).catch(() => void 0);
  }, []);

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead><tr><th>TemplateId</th><th>Name</th><th>Description</th><th>ImplementationSteps</th><th>BackoutPlan</th><th>ServiceSystem</th><th>Category</th><th>Environment</th><th>ChangeTypeId</th><th>RiskLevelId</th><th>IsActive</th><th>CreatedAt</th><th>CreatedBy</th></tr></thead>
        <tbody>
          {templates.map((template) => (
            <tr key={template.templateId}>
              <td>{template.templateId}</td>
              <td>{template.name}</td>
              <td>{template.description || "—"}</td>
              <td>{template.implementationSteps || "—"}</td>
              <td>{template.backoutPlan || "—"}</td>
              <td>{template.serviceSystem || "—"}</td>
              <td>{template.category || "—"}</td>
              <td>{template.environment || "—"}</td>
              <td>{template.changeTypeId ? `${template.changeTypeId} (${template.changeTypeName ?? "Unknown"})` : "—"}</td>
              <td>{template.riskLevelId ? `${template.riskLevelId} (${template.riskLevelName ?? "Unknown"})` : "—"}</td>
              <td>{template.isActive ? "Yes" : "No"}</td>
              <td>{formatDate(template.createdAt)}</td>
              <td>{template.createdByDisplayName || template.createdByUpn || template.createdBy || "—"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AdminTemplatesTab;
