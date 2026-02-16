import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { ChangeCreateDto, ChangeRequest, ChangeStatus, ChangeType, RiskLevel } from "../types/change";

type Tab = "Basic Info" | "Schedule" | "Risk & Impact" | "Implementation Plan" | "Attachments" | "Approval";
const tabs: Tab[] = ["Basic Info", "Schedule", "Risk & Impact", "Implementation Plan", "Attachments", "Approval"];
const statusRank: Record<ChangeStatus, number> = { Draft: 1, Submitted: 2, Approved: 3, Rejected: 4, InImplementation: 5, Completed: 6, Closed: 7 };

const ChangeDetailPage = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isNew = !id;

  const [tab, setTab] = useState<Tab>("Basic Info");
  const [change, setChange] = useState<ChangeRequest | null>(null);
  const [form, setForm] = useState<ChangeCreateDto>({ title: "", description: "", impactDescription: "", rollbackPlan: "" });

  const locked = !!change && statusRank[change.status] >= statusRank.Approved;
  const canSubmit = !!form.title && !!form.changeType && !!form.riskLevel && !!form.implementationDate && !!form.impactDescription && !!form.rollbackPlan;

  useEffect(() => {
    if (!id) return;
    apiClient.getChangeById(id).then((data) => {
      setChange(data);
      setForm({
        title: data.title,
        description: data.description,
        changeType: data.changeType,
        riskLevel: data.riskLevel,
        implementationDate: data.implementationDate,
        impactDescription: data.impactDescription,
        rollbackPlan: data.rollbackPlan,
        assignedToUserId: data.assignedToUserId
      });
    }).catch(() => setChange(null));
  }, [id]);

  const save = async () => {
    if (isNew) {
      const created = await apiClient.createChange(form);
      navigate(`/changes/${created.changeId}`);
      return;
    }

    if (id) {
      const updated = await apiClient.updateChange(id, form);
      setChange(updated);
    }
  };

  const statusActions = useMemo(() => {
    if (!change) return [] as { label: string; run: () => Promise<ChangeRequest> }[];
    return [
      { label: "Submit", run: () => apiClient.submitChange(change.changeId) },
      { label: "Approve", run: () => apiClient.approveChange(change.changeId) },
      { label: "Reject", run: () => apiClient.rejectChange(change.changeId) },
      { label: "Start", run: () => apiClient.startChange(change.changeId) },
      { label: "Complete", run: () => apiClient.completeChange(change.changeId) }
    ];
  }, [change]);

  return (
    <div>
      <div className="page-head">
        <h1 className="page-title">{isNew ? "New Change" : `CHG-${String(change?.changeNumber ?? 0).padStart(6, "0")}`}</h1>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="btn" disabled={!canSubmit || locked} onClick={save}>Save</button>
          <button className="btn btn-primary" disabled={!canSubmit || locked || !change} onClick={() => change && apiClient.submitChange(change.changeId).then(setChange)}>Submit</button>
        </div>
      </div>

      <div className="tabs">{tabs.map((x) => <button key={x} className={`tab ${tab === x ? "tab-active" : ""}`} onClick={() => setTab(x)}>{x}</button>)}</div>

      <div className="card card-pad" style={{ marginTop: 12 }}>
        {tab === "Basic Info" && <div className="form-grid">
          <input className="input" placeholder="Title" disabled={locked} value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
          <select className="select" disabled={locked} value={form.changeType ?? ""} onChange={(e) => setForm({ ...form, changeType: e.target.value as ChangeType })}><option value="">Change Type</option><option>Standard</option><option>Emergency</option></select>
          <textarea className="textarea" placeholder="Description" disabled={locked} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </div>}

        {tab === "Schedule" && <input className="input" type="datetime-local" disabled={locked} value={(form.implementationDate ?? "").slice(0, 16)} onChange={(e) => setForm({ ...form, implementationDate: e.target.value })} />}

        {tab === "Risk & Impact" && <div className="form-grid">
          <select className="select" disabled={locked} value={form.riskLevel ?? ""} onChange={(e) => setForm({ ...form, riskLevel: e.target.value as RiskLevel })}><option value="">Risk Level</option><option>Low</option><option>Medium</option><option>High</option><option>Critical</option></select>
          <textarea className="textarea" disabled={locked} placeholder="Impact Description" value={form.impactDescription} onChange={(e) => setForm({ ...form, impactDescription: e.target.value })} />
        </div>}

        {tab === "Implementation Plan" && <textarea className="textarea" disabled={locked} placeholder="Rollback Plan" value={form.rollbackPlan} onChange={(e) => setForm({ ...form, rollbackPlan: e.target.value })} />}

        {tab === "Attachments" && change && <input type="file" onChange={(e) => e.target.files?.[0] && apiClient.uploadAttachment(change.changeId, e.target.files[0])} />}

        {tab === "Approval" && change && <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
          {statusActions.map((a) => <button key={a.label} className="btn" onClick={() => a.run().then(setChange)}>{a.label}</button>)}
        </div>}
      </div>
    </div>
  );
};

export default ChangeDetailPage;
