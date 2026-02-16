import { useState } from "react";

type Template = {
  id: string;
  name: string;
  tags: string[];
  risk: "Low" | "Medium" | "High";
  description: string;
  stepsDefined?: boolean;
  cabRequired?: boolean;
};

const riskPill = (risk: Template["risk"]) => {
  if (risk === "Low") return "pill pill-green";
  if (risk === "High") return "pill pill-red";
  return "pill pill-amber";
};

const seed: Template[] = [
  {
    id: "tpl-001",
    name: "Windows Security Patch",
    tags: ["Standard", "Server"],
    risk: "Low",
    description:
      "Deploy monthly Windows security patches to [SERVER_GROUP] servers as part of regular patch cycle.",
    stepsDefined: true
  },
  {
    id: "tpl-002",
    name: "Network Firewall Rule Change",
    tags: ["Normal", "Network"],
    risk: "Medium",
    description:
      "Modify firewall rules on [FIREWALL_NAME] to allow/block traffic for [SERVICE/APPLICATION].",
    stepsDefined: true,
    cabRequired: true
  },
  {
    id: "tpl-003",
    name: "Database Maintenance",
    tags: ["Standard", "Database"],
    risk: "Low",
    description:
      "Perform scheduled database maintenance including index rebuild and statistics update on [DATABASE_NAME].",
    stepsDefined: true
  }
];

const TemplatesPage = () => {
  const [items, setItems] = useState<Template[]>(seed);

  const duplicate = (id: string) => {
    const t = items.find((x) => x.id === id);
    if (!t) return;
    setItems([{ ...t, id: `tpl-${Math.random().toString(16).slice(2, 6)}`, name: `${t.name} (Copy)` }, ...items]);
  };

  const remove = (id: string) => setItems(items.filter((x) => x.id !== id));

  return (
    <div>
      <div className="page-head">
        <div>
          <h1 className="page-title">Change Templates</h1>
          <p className="page-subtitle">Pre-defined templates for common change types</p>
        </div>

        <button className="btn btn-primary" onClick={() => alert("Template create/edit can be wired next (API + form).")}>
          + New Template
        </button>
      </div>

      <div className="grid grid-2">
        {items.map((t) => (
          <div key={t.id} className="card">
            <div className="card-pad">
              <div className="row">
                <div className="row-left">
                  <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                    <span className="pill pill-blue">📄</span>
                    <div className="h3">{t.name}</div>
                  </div>
                </div>

                <div style={{ display: "flex", gap: 10 }}>
                  <button className="iconbtn" title="Duplicate" onClick={() => duplicate(t.id)}>
                    ⧉
                  </button>
                  <button className="iconbtn" title="Edit" onClick={() => alert("Edit UI next (form modal).")}>
                    ✎
                  </button>
                  <button className="iconbtn" title="Delete" onClick={() => remove(t.id)}>
                    🗑
                  </button>
                </div>
              </div>

              <div style={{ marginTop: 10, display: "flex", gap: 8, flexWrap: "wrap" }}>
                {t.tags.map((tag) => (
                  <span key={tag} className="pill">
                    {tag}
                  </span>
                ))}
                <span className={riskPill(t.risk)}>Risk: {t.risk}</span>
              </div>

              <div className="small" style={{ marginTop: 10 }}>{t.description}</div>

              <div style={{ marginTop: 12, display: "flex", gap: 12, flexWrap: "wrap" }}>
                {t.cabRequired ? <span className="pill pill-amber">⚠ CAB Required</span> : null}
                {t.stepsDefined ? <span className="pill pill-green">✓ Steps defined</span> : <span className="pill">Steps not defined</span>}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default TemplatesPage;
