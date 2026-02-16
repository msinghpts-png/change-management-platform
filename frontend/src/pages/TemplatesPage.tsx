import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { ChangeTemplate } from "../types/change";

const riskPill = (value: string) => {
  const risk = value.toLowerCase();
  if (risk === "low") return "pill pill-green";
  if (risk === "high") return "pill pill-red";
  return "pill pill-amber";
};

const TemplatesPage = () => {
  const [items, setItems] = useState<ChangeTemplate[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = async () => {
    setLoading(true);
    setError(null);
    try {
      setItems(await apiClient.getTemplates());
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refresh().catch(() => void 0);
  }, []);

  const createTemplate = async () => {
    const name = window.prompt("Template name");
    if (!name?.trim()) return;

    await apiClient.createTemplate({
      name: name.trim(),
      description: "",
      implementationSteps: "",
      backoutPlan: "",
      serviceSystem: "",
      category: "Application",
      environment: "Non-Production",
      businessJustification: ""
    });

    await refresh();
  };

  const editTemplate = async (item: ChangeTemplate) => {
    const name = window.prompt("Template name", item.name);
    if (!name?.trim()) return;

    await apiClient.updateTemplate(item.templateId, {
      name: name.trim(),
      description: item.description,
      implementationSteps: item.implementationSteps,
      backoutPlan: item.backoutPlan,
      serviceSystem: item.serviceSystem,
      category: item.category,
      environment: item.environment,
      businessJustification: item.businessJustification,
      isActive: item.isActive
    });

    await refresh();
  };

  const remove = async (id: string) => {
    await apiClient.deleteTemplate(id);
    await refresh();
  };

  return (
    <div>
      <div className="page-head">
        <div>
          <h1 className="page-title">Change Templates</h1>
          <p className="page-subtitle">Pre-defined templates for common change types</p>
        </div>

        <button className="btn btn-primary" onClick={createTemplate}>
          + New Template
        </button>
      </div>

      {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)", marginBottom: 12 }}>{error}</div> : null}

      <div className="grid grid-2">
        {items.map((t) => (
          <div key={t.templateId} className="card">
            <div className="card-pad">
              <div className="row">
                <div className="row-left">
                  <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                    <span className="pill pill-blue">📄</span>
                    <div className="h3">{t.name}</div>
                  </div>
                </div>

                <div style={{ display: "flex", gap: 10 }}>
                  <button className="iconbtn" title="Edit" onClick={() => editTemplate(t)}>
                    ✎
                  </button>
                  <button className="iconbtn" title="Delete" onClick={() => remove(t.templateId)}>
                    🗑
                  </button>
                </div>
              </div>

              <div style={{ marginTop: 10, display: "flex", gap: 8, flexWrap: "wrap" }}>
                {t.category ? <span className="pill">{t.category}</span> : null}
                {t.environment ? <span className="pill">{t.environment}</span> : null}
                <span className={riskPill(t.description?.toLowerCase().includes("high") ? "High" : "Medium")}>Template</span>
              </div>

              <div className="small" style={{ marginTop: 10 }}>{t.description || "No description"}</div>

              <div style={{ marginTop: 12, display: "flex", gap: 12, flexWrap: "wrap" }}>
                {t.backoutPlan ? <span className="pill pill-green">✓ Backout plan defined</span> : <span className="pill">Backout pending</span>}
                {t.implementationSteps ? <span className="pill pill-green">✓ Steps defined</span> : <span className="pill">Steps not defined</span>}
              </div>
            </div>
          </div>
        ))}
      </div>

      {loading ? <div className="small" style={{ marginTop: 8 }}>Loading templates…</div> : null}
      {!loading && !items.length ? <div className="empty">No templates yet.</div> : null}
    </div>
  );
};

export default TemplatesPage;
