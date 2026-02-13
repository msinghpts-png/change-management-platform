import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { Approval, ApprovalStatus, Attachment, ChangeCreateDto, ChangeRequest, ChangeUpdateDto } from "../types/change";

type ViewTab = "Overview" | "Approvals" | "Tasks" | "Attachments";
type FormTab = "Basic Info" | "Schedule" | "Plans" | "Risk & Impact";

const pillForStatus = (status?: string) => {
  const s = (status ?? "").toLowerCase();
  if (s.includes("approved")) return "pill pill-green";
  if (s.includes("closed") || s.includes("complete")) return "pill pill-green";
  if (s.includes("pending")) return "pill pill-amber";
  if (s.includes("inprogress") || s.includes("in progress") || s.includes("implementation"))
    return "pill pill-cyan";
  if (s.includes("rejected")) return "pill pill-red";
  return "pill";
};

const pillForPriority = (priority?: string) => {
  const p = (priority ?? "").toLowerCase();
  if (p.includes("p1") || p.includes("emergency")) return "pill pill-red";
  if (p.includes("p2")) return "pill pill-amber";
  if (p.includes("p3")) return "pill pill-blue";
  return "pill";
};

const fmtDT = (value?: string) => {
  if (!value) return "—";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return "—";
  return d.toLocaleString(undefined, { year: "numeric", month: "short", day: "numeric", hour: "numeric", minute: "2-digit" });
};

const templates = [
  {
    id: "tpl-windows-patch",
    name: "Windows Security Patch",
    category: "Server",
    risk: "Low",
    description: "Deploy monthly Windows security patches to [SERVER_GROUP] as part of regular patch cycle.",
    steps: "1. Take VM snapshots\n2. Disable servers in load balancer (rolling)\n3. Install patches via WSUS\n4. Reboot\n5. Validate services\n6. Re-enable in load balancer"
  },
  {
    id: "tpl-firewall",
    name: "Network Firewall Rule Change",
    category: "Network",
    risk: "Medium",
    description: "Modify firewall rules on [FIREWALL_NAME] to allow/block traffic for [SERVICE/APPLICATION].",
    steps: "1. Export current config\n2. Apply rule changes\n3. Validate connectivity\n4. Monitor logs"
  },
  {
    id: "tpl-db-maint",
    name: "Database Maintenance",
    category: "Database",
    risk: "Low",
    description: "Perform scheduled database maintenance including index rebuild and statistics update on [DATABASE_NAME].",
    steps: "1. Confirm maintenance window\n2. Run index/statistics jobs\n3. Validate performance\n4. Confirm backups"
  }
];

const ChangeDetailPage = () => {
  const nav = useNavigate();
  const { id } = useParams();

  const isNew = !id;

  const [tab, setTab] = useState<ViewTab>("Overview");
  const [formTab, setFormTab] = useState<FormTab>("Basic Info");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [item, setItem] = useState<ChangeRequest | null>(null);

  const [approvals, setApprovals] = useState<Approval[]>([]);
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [approverEmail, setApproverEmail] = useState("");
  const [approvalComment, setApprovalComment] = useState("");
  const [decisionComment, setDecisionComment] = useState("");
  const [draggingAttachment, setDraggingAttachment] = useState(false);

  // Form fields (kept in UI; backend DTO is smaller)
  const [title, setTitle] = useState("");
  const [category, setCategory] = useState("Application");
  const [environment, setEnvironment] = useState("Non-Production");
  const [service, setService] = useState("");
  const [description, setDescription] = useState("");
  const [businessJustification, setBusinessJustification] = useState("");
  const [implementationSteps, setImplementationSteps] = useState("");
  const [backoutPlan, setBackoutPlan] = useState("");

  const [priority, setPriority] = useState("P3");
  const [riskLevel, setRiskLevel] = useState("Medium");
  const [impactLevel, setImpactLevel] = useState("Medium");

  const [plannedStart, setPlannedStart] = useState("");
  const [plannedEnd, setPlannedEnd] = useState("");
  const [downtimeRequired, setDowntimeRequired] = useState(false);

  const [templateId, setTemplateId] = useState("");


  const refreshRelatedData = async (changeId: string) => {
    const [nextApprovals, nextAttachments] = await Promise.all([
      apiClient.getApprovals(changeId),
      apiClient.getAttachments(changeId)
    ]);

    setApprovals(nextApprovals ?? []);
    setAttachments(nextAttachments ?? []);
  };

  useEffect(() => {
    if (!id) return;

    setLoading(true);
    apiClient
      .getChangeById(id)
      .then((data) => {
        setItem(data);
        setTitle(data.title ?? "");
        setDescription(data.description ?? "");
        setPriority(data.priority ?? "P3");
        setRiskLevel(data.riskLevel ?? "Medium");
        setImpactLevel(data.impactLevel ?? "Medium");
        setPlannedStart(data.plannedStart ? data.plannedStart.slice(0, 16) : "");
        setPlannedEnd(data.plannedEnd ? data.plannedEnd.slice(0, 16) : "");
        refreshRelatedData(id).catch(() => void 0);
        setLoading(false);
      })
      .catch((err: Error) => {
        setError(err.message);
        refreshRelatedData(id).catch(() => void 0);
        setLoading(false);
      });
  }, [id]);

  const compiledDescription = useMemo(() => {
    // Keep it readable in the DB until backend supports first-class fields.
    const parts = [
      description?.trim() ? `Description:\n${description.trim()}` : "",
      businessJustification?.trim() ? `\nBusiness Justification:\n${businessJustification.trim()}` : "",
      implementationSteps?.trim() ? `\nImplementation Steps:\n${implementationSteps.trim()}` : "",
      backoutPlan?.trim() ? `\nBackout Plan:\n${backoutPlan.trim()}` : "",
      service?.trim() ? `\nService/System: ${service.trim()}` : "",
      category ? `\nCategory: ${category}` : "",
      environment ? `\nEnvironment: ${environment}` : "",
      downtimeRequired ? `\nDowntime Required: Yes` : ""
    ].filter(Boolean);

    return parts.join("\n");
  }, [description, businessJustification, implementationSteps, backoutPlan, service, category, environment, downtimeRequired]);

  const applyTemplate = (tplId: string) => {
    setTemplateId(tplId);
    const tpl = templates.find((t) => t.id === tplId);
    if (!tpl) return;
    setCategory(tpl.category);
    setRiskLevel(tpl.risk);
    if (!title) setTitle(tpl.name);
    if (!description) setDescription(tpl.description);
    if (!implementationSteps) setImplementationSteps(tpl.steps);
  };

  const saveDraft = async () => {
    setError(null);
    setLoading(true);
    try {
      if (isNew) {
        const payload: ChangeCreateDto = {
          title,
          description: compiledDescription,
          priority,
          riskLevel,
          impactLevel,
          plannedStart: plannedStart ? new Date(plannedStart).toISOString() : undefined,
          plannedEnd: plannedEnd ? new Date(plannedEnd).toISOString() : undefined
        };
        const created = await apiClient.createChange(payload);
        nav(`/changes/${created.id}`);
      } else if (id) {
        const payload: ChangeUpdateDto = {
          title,
          description: compiledDescription,
          priority,
          riskLevel,
          impactLevel,
          plannedStart: plannedStart ? new Date(plannedStart).toISOString() : undefined,
          plannedEnd: plannedEnd ? new Date(plannedEnd).toISOString() : undefined
        };
        const updated = await apiClient.updateChange(id, payload);
        setItem(updated);
        await refreshRelatedData(id);
      }
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const submitForApproval = async () => {
    if (!id && isNew) {
      await saveDraft();
      return;
    }
    if (!id) return;

    setError(null);
    setLoading(true);
    try {
      await apiClient.submitChange(id);
      const refreshed = await apiClient.getChangeById(id);
      setItem(refreshed);
      await refreshRelatedData(id);
      alert("Submitted for approval.");
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const submitApproval = async () => {
    if (!id || !approverEmail.trim()) return;
    setLoading(true);
    setError(null);
    try {
      await apiClient.createApproval(id, { approver: approverEmail.trim(), comment: approvalComment.trim() || undefined });
      setApproverEmail("");
      setApprovalComment("");
      await refreshRelatedData(id);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const decideApproval = async (approvalId: string, status: ApprovalStatus) => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      await apiClient.decideApproval(id, approvalId, { status, comment: decisionComment.trim() || undefined });
      const refreshed = await apiClient.getChangeById(id);
      setItem(refreshed);
      setDecisionComment("");
      await refreshRelatedData(id);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const uploadAttachment = async (event: any) => {
    if (!id || !event.target.files?.[0]) return;
    setLoading(true);
    setError(null);
    try {
      await apiClient.uploadAttachment(id, event.target.files[0]);
      await refreshRelatedData(id);
      event.target.value = "";
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };


  const onDropAttachment = async (event: any) => {
    event.preventDefault();
    setDraggingAttachment(false);
    if (!id || !event.dataTransfer?.files?.[0]) return;

    setLoading(true);
    setError(null);
    try {
      await apiClient.uploadAttachment(id, event.dataTransfer.files[0]);
      await refreshRelatedData(id);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  if (loading && !item && !isNew) {
    return <div className="card card-pad">Loading…</div>;
  }

  if (!isNew && !item && error) {
    return <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div>;
  }

  // ---------- NEW / EDIT FORM ----------
  if (isNew || (!isNew && item && (item.status ?? "").toLowerCase().includes("draft"))) {
    return (
      <div>
        <button className="btn" onClick={() => nav("/dashboard")} style={{ marginBottom: 12 }}>
          ← Back to Dashboard
        </button>

        <div className="page-head" style={{ marginTop: 0 }}>
          <div>
            <h1 className="page-title">{isNew ? "New Change Request" : "Edit Change Request"}</h1>
            <p className="page-subtitle">Create a new change request for approval</p>
          </div>
        </div>

        {error ? <div className="card card-pad" style={{ borderColor: "rgba(220,38,38,.35)" }}>{error}</div> : null}

        <div className="card card-pad" style={{ borderStyle: "dashed" }}>
          <div className="row">
            <div>
              <div className="h3">Start from a template</div>
              <div className="small">Pre-fill with standard change patterns</div>
            </div>
            <select className="select" value={templateId} onChange={(e) => applyTemplate(e.target.value)}>
              <option value="">Select template…</option>
              {templates.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div style={{ height: 12 }} />

        <div className="card">
          <div className="card-head">
            <div className="tabs" role="tablist" aria-label="Change form sections">
              {(["Basic Info", "Schedule", "Plans", "Risk & Impact"] as FormTab[]).map((t) => (
                <button
                  key={t}
                  className={"tab " + (formTab === t ? "tab-active" : "")}
                  onClick={() => setFormTab(t)}
                  type="button"
                >
                  {t}
                </button>
              ))}
            </div>
          </div>

          <div className="card-body">
            {formTab === "Basic Info" ? (
              <div className="form-grid">
                <div style={{ gridColumn: "1 / -1" }}>
                  <div className="label">Title *</div>
                  <input className="input" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Brief title for the change" />
                </div>

                <div>
                  <div className="label">Change Type *</div>
                  <select className="select" value="Normal" onChange={() => void 0}>
                    <option>Normal</option>
                    <option>Standard</option>
                    <option>Emergency</option>
                  </select>
                </div>

                <div>
                  <div className="label">Category *</div>
                  <select className="select" value={category} onChange={(e) => setCategory(e.target.value)}>
                    <option>Application</option>
                    <option>Server</option>
                    <option>Network</option>
                    <option>Database</option>
                    <option>Security</option>
                  </select>
                </div>

                <div>
                  <div className="label">Service / System</div>
                  <input className="input" value={service} onChange={(e) => setService(e.target.value)} placeholder="Affected service or system" />
                </div>

                <div>
                  <div className="label">Environment *</div>
                  <select className="select" value={environment} onChange={(e) => setEnvironment(e.target.value)}>
                    <option>Non-Production</option>
                    <option>Production</option>
                    <option>DR</option>
                  </select>
                </div>

                <div style={{ gridColumn: "1 / -1" }}>
                  <div className="label">Description *</div>
                  <textarea className="textarea" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Detailed description of what will be changed…" />
                </div>

                <div style={{ gridColumn: "1 / -1" }}>
                  <div className="label">Business Justification *</div>
                  <textarea className="textarea" value={businessJustification} onChange={(e) => setBusinessJustification(e.target.value)} placeholder="Why is this change needed? What business value does it provide?" />
                </div>
              </div>
            ) : null}

            {formTab === "Schedule" ? (
              <div className="form-grid">
                <div>
                  <div className="label">Planned Start</div>
                  <input className="input" type="datetime-local" value={plannedStart} onChange={(e) => setPlannedStart(e.target.value)} />
                </div>
                <div>
                  <div className="label">Planned End</div>
                  <input className="input" type="datetime-local" value={plannedEnd} onChange={(e) => setPlannedEnd(e.target.value)} />
                </div>
                <div style={{ gridColumn: "1 / -1" }}>
                  <label style={{ display: "flex", alignItems: "center", gap: 10, fontWeight: 700 }}>
                    <input type="checkbox" checked={downtimeRequired} onChange={(e) => setDowntimeRequired(e.target.checked)} />
                    Downtime required
                  </label>
                </div>
              </div>
            ) : null}

            {formTab === "Plans" ? (
              <div className="form-grid">
                <div style={{ gridColumn: "1 / -1" }}>
                  <div className="label">Implementation Steps</div>
                  <textarea className="textarea" value={implementationSteps} onChange={(e) => setImplementationSteps(e.target.value)} placeholder="1. Step one\n2. Step two…" />
                </div>
                <div style={{ gridColumn: "1 / -1" }}>
                  <div className="label">Backout Plan</div>
                  <textarea className="textarea" value={backoutPlan} onChange={(e) => setBackoutPlan(e.target.value)} placeholder="1. Revert snapshot\n2. Rollback config…" />
                </div>
              </div>
            ) : null}

            {formTab === "Risk & Impact" ? (
              <div className="form-grid">
                <div>
                  <div className="label">Priority</div>
                  <select className="select" value={priority} onChange={(e) => setPriority(e.target.value)}>
                    <option>P1</option>
                    <option>P2</option>
                    <option>P3</option>
                    <option>P4</option>
                  </select>
                </div>
                <div>
                  <div className="label">Risk</div>
                  <select className="select" value={riskLevel} onChange={(e) => setRiskLevel(e.target.value)}>
                    <option>Low</option>
                    <option>Medium</option>
                    <option>High</option>
                  </select>
                </div>
                <div>
                  <div className="label">Impact</div>
                  <select className="select" value={impactLevel} onChange={(e) => setImpactLevel(e.target.value)}>
                    <option>Low</option>
                    <option>Medium</option>
                    <option>High</option>
                  </select>
                </div>
                <div />
              </div>
            ) : null}
          </div>

          <div className="footer-actions">
            <button className="btn" onClick={saveDraft} disabled={loading}>
              💾 Save Draft
            </button>
            <button className="btn btn-primary" onClick={submitForApproval} disabled={loading || !title.trim()}>
              ✈ Submit for Approval
            </button>
          </div>
        </div>
      </div>
    );
  }

  // ---------- DETAIL VIEW ----------
  const viewTabs: ViewTab[] = ["Overview", "Approvals", "Tasks", "Attachments"];

  return (
    <div>
      <button className="btn" onClick={() => nav("/changes")} style={{ marginBottom: 12 }}>
        ← Back to list
      </button>

      <div className="page-head" style={{ marginTop: 0 }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: 10, flexWrap: "wrap" }}>
            <span className="mono">{item?.changeNumber ?? "CHG-000000"}</span>
            <span className={pillForStatus(item?.status)}>{item?.status ?? "—"}</span>
          </div>

          <h1 className="page-title" style={{ marginTop: 10 }}>{item?.title}</h1>

          <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginTop: 10 }}>
            <span className={pillForPriority(item?.priority)}>{item?.priority ?? "P3"}</span>
            <span className="pill pill-blue">Normal</span>
            {item?.riskLevel ? <span className="pill pill-amber">Risk: {item.riskLevel}</span> : null}
            {item?.impactLevel ? <span className="pill pill-amber">Impact: {item.impactLevel}</span> : null}
          </div>
        </div>

        <button className="btn" onClick={() => alert("Implementation workflow can be wired next (status transition + audit).")}>
          ▶ Start Implementation
        </button>
      </div>

      <div className="tabs" style={{ marginBottom: 14 }}>
        {viewTabs.map((t) => (
          <button key={t} className={"tab " + (tab === t ? "tab-active" : "")} onClick={() => setTab(t)}>
            {t}
          </button>
        ))}
      </div>

      {tab === "Overview" ? (
        <div className="two-col">
          <div className="grid" style={{ gap: 12 }}>
            <div className="card">
              <div className="card-head">
                <div className="card-title">Description</div>
              </div>
              <div className="card-body">
                <pre style={{ margin: 0, whiteSpace: "pre-wrap", fontFamily: "inherit" }}>{item?.description ?? "—"}</pre>
              </div>
            </div>

            <div className="card">
              <div className="card-head">
                <div className="card-title">Implementation Steps</div>
              </div>
              <div className="card-body">
                <div className="small">This section is currently stored in the description until backend fields are added.</div>
              </div>
            </div>

            <div className="card">
              <div className="card-head">
                <div className="card-title">Backout Plan</div>
              </div>
              <div className="card-body">
                <div className="small">This section is currently stored in the description until backend fields are added.</div>
              </div>
            </div>
          </div>

          <div className="grid" style={{ gap: 12 }}>
            <div className="card">
              <div className="card-head">
                <div className="card-title">Details</div>
              </div>
              <div className="card-body">
                <div className="small">Category</div>
                <div className="h3">{item?.category ?? "—"}</div>
                <div style={{ height: 10 }} />
                <div className="small">Environment</div>
                <div className="h3">{item?.environment ?? "—"}</div>
                <div style={{ height: 10 }} />
                <div className="small">Service</div>
                <div className="h3">{item?.service ?? "—"}</div>
              </div>
            </div>

            <div className="card">
              <div className="card-head">
                <div className="card-title">Schedule</div>
              </div>
              <div className="card-body">
                <div className="small">Planned Start</div>
                <div className="h3">{fmtDT(item?.plannedStart)}</div>
                <div style={{ height: 10 }} />
                <div className="small">Planned End</div>
                <div className="h3">{fmtDT(item?.plannedEnd)}</div>
                <div style={{ height: 10 }} />
                <div className="pill pill-amber">⚠ Downtime Required</div>
              </div>
            </div>

            <div className="card">
              <div className="card-head">
                <div className="card-title">People</div>
              </div>
              <div className="card-body">
                <div className="small">Owner</div>
                <div className="h3">{item?.requestedBy ?? "admin@example.com"}</div>
              </div>
            </div>
          </div>
        </div>
      ) : null}

      {tab === "Approvals" ? (
        <div className="grid" style={{ gap: 12 }}>
          <div className="card card-pad">
            <div className="h3">Submit approval request</div>
            <div className="form-grid" style={{ marginTop: 8 }}>
              <div><input className="input" placeholder="Approver email" value={approverEmail} onChange={(e) => setApproverEmail(e.target.value)} /></div>
              <div><input className="input" placeholder="Comment" value={approvalComment} onChange={(e) => setApprovalComment(e.target.value)} /></div>
            </div>
            <div style={{ marginTop: 8 }}><button className="btn btn-primary" disabled={!approverEmail.trim() || loading} onClick={submitApproval}>Add Approval</button></div>
          </div>
          <div className="card card-pad">
            <div className="h3">Approval history</div>
            <input className="input" style={{ marginTop: 8 }} placeholder="Decision comment" value={decisionComment} onChange={(e) => setDecisionComment(e.target.value)} />
            <div style={{ display: "grid", gap: 8, marginTop: 10 }}>
              {approvals.map((approval) => (
                <div key={approval.id} className="row">
                  <div className="row-left">
                    <div className="h3">{approval.approver}</div>
                    <div className="small">{approval.comment ?? "No comment"}</div>
                  </div>
                  <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                    <span className={pillForStatus(approval.status)}>{approval.status}</span>
                    {approval.status === "Pending" ? (<>
                      <button className="btn" onClick={() => decideApproval(approval.id, "Approved")}>Approve</button>
                      <button className="btn" onClick={() => decideApproval(approval.id, "Rejected")}>Reject</button>
                    </>) : null}
                  </div>
                </div>
              ))}
              {!approvals.length ? <div className="empty">No approvals yet.</div> : null}
            </div>
          </div>
        </div>
      ) : null}

      {tab === "Attachments" ? (
        <div className="card card-pad">
          <div className="h3">Attachments</div>
          <div className="small">Allowed: pdf, doc(x), xls(x), png, jpg. Max 10 MB.</div>
          <div
            className="card card-pad"
            style={{ marginTop: 8, borderStyle: "dashed", background: draggingAttachment ? "rgba(59,130,246,.12)" : undefined }}
            onDragOver={(event) => { event.preventDefault(); setDraggingAttachment(true); }}
            onDragLeave={() => setDraggingAttachment(false)}
            onDrop={onDropAttachment}
          >
            Drag & drop a file here or choose below.
            <input className="input" style={{ marginTop: 8 }} type="file" onChange={uploadAttachment} />
          </div>
          <div style={{ display: "grid", gap: 8, marginTop: 12 }}>
            {attachments.map((attachment) => (
              <div key={attachment.id} className="row">
                <div className="row-left">
                  <div className="h3">{attachment.fileName}</div>
                  <div className="small">{Math.round(attachment.sizeBytes / 1024)} KB</div>
                </div>
                <a className="btn" href={`/api/changes/${id}/attachments/${attachment.id}`}>Download</a>
              </div>
            ))}
            {!attachments.length ? <div className="empty">No attachments uploaded.</div> : null}
          </div>
        </div>
      ) : null}

      {tab === "Tasks" ? (
        <div className="card">
          <div className="empty">Tasks workflow can be wired next.</div>
        </div>
      ) : null}
    </div>
  );
};

export default ChangeDetailPage;
