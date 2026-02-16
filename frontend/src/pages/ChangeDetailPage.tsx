import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { Approval, ApprovalStatus, Attachment, ChangeCreateDto, ChangeRequest, ChangeTask, ChangeTemplate, ChangeUpdateDto } from "../types/change";

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

const fallbackTemplates: ChangeTemplate[] = [
  {
    templateId: "00000000-0000-0000-0000-000000000001",
    name: "Windows Security Patch",
    category: "Server",
    description: "Deploy monthly Windows security patches to [SERVER_GROUP] as part of regular patch cycle.",
    implementationSteps: "1. Take VM snapshots\n2. Disable servers in load balancer (rolling)\n3. Install patches via WSUS\n4. Reboot\n5. Validate services\n6. Re-enable in load balancer",
    serviceSystem: "",
    environment: "Non-Production",
    businessJustification: "",
    backoutPlan: "",
    isActive: true
  },
  {
    templateId: "00000000-0000-0000-0000-000000000002",
    name: "Network Firewall Rule Change",
    category: "Network",
    description: "Modify firewall rules on [FIREWALL_NAME] to allow/block traffic for [SERVICE/APPLICATION].",
    implementationSteps: "1. Export current config\n2. Apply rule changes\n3. Validate connectivity\n4. Monitor logs",
    serviceSystem: "",
    environment: "Non-Production",
    businessJustification: "",
    backoutPlan: "",
    isActive: true
  },
  {
    templateId: "00000000-0000-0000-0000-000000000003",
    name: "Database Maintenance",
    category: "Database",
    description: "Perform scheduled database maintenance including index rebuild and statistics update on [DATABASE_NAME].",
    implementationSteps: "1. Confirm maintenance window\n2. Run index/statistics jobs\n3. Validate performance\n4. Confirm backups",
    serviceSystem: "",
    environment: "Non-Production",
    businessJustification: "",
    backoutPlan: "",
    isActive: true
  }
];

const priorityToId = (priority: string) => {
  const normalized = priority.trim().toUpperCase();
  if (normalized === "P1") return 4;
  if (normalized === "P2") return 3;
  if (normalized === "P4") return 1;
  return 2;
};

const riskToId = (risk: string) => {
  const normalized = risk.trim().toLowerCase();
  if (normalized === "low") return 1;
  if (normalized === "high") return 3;
  return 2;
};

const ChangeDetailPage = () => {
  const nav = useNavigate();
  const { id } = useParams();

  const hasValidId = apiClient.isValidId(id);
  const isNew = !hasValidId;

  const [tab, setTab] = useState<ViewTab>("Overview");
  const [formTab, setFormTab] = useState<FormTab>("Basic Info");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [item, setItem] = useState<ChangeRequest | null>(null);

  const [approvals, setApprovals] = useState<Approval[]>([]);
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [tasks, setTasks] = useState<ChangeTask[]>([]);
  const [taskTitle, setTaskTitle] = useState("");
  const [taskDescription, setTaskDescription] = useState("");
  const [approverEmail, setApproverEmail] = useState("");
  const [approvalComment, setApprovalComment] = useState("");
  const [decisionComment, setDecisionComment] = useState("");

  // Form fields (kept in UI; backend DTO is smaller)
  const [title, setTitle] = useState("");
  const [category, setCategory] = useState("Application");
  const [environment, setEnvironment] = useState("Non-Production");
  const [service, setService] = useState("");
  const [description, setDescription] = useState("");
  const [businessJustification, setBusinessJustification] = useState("");
  const [implementationSteps, setImplementationSteps] = useState("");
  const [backoutPlan, setBackoutPlan] = useState("");

  const [changeType, setChangeType] = useState("Normal");
  const [priority, setPriority] = useState("P3");
  const [riskLevel, setRiskLevel] = useState("Medium");
  const [impactLevel, setImpactLevel] = useState("Medium");

  const [plannedStart, setPlannedStart] = useState("");
  const [plannedEnd, setPlannedEnd] = useState("");
  const [downtimeRequired, setDowntimeRequired] = useState(false);

  const [templateId, setTemplateId] = useState("");
  const [templates, setTemplates] = useState<ChangeTemplate[]>(fallbackTemplates);

  const changeTypeId = changeType === "Standard" ? 1 : changeType === "Emergency" ? 3 : 2;
  const implementationDate = plannedStart;
  const impactDescription = description;
  const rollbackPlan = backoutPlan;

  const isSubmitReady = Boolean(
    title.trim() &&
    changeTypeId &&
    riskLevel &&
    implementationDate &&
    impactDescription.trim() &&
    rollbackPlan.trim()
  );
  const isSubmitDisabled = loading || !isSubmitReady;
  const submitBlockers: string[] = [];
  if (!title.trim()) submitBlockers.push("Title is required");
  if (!changeTypeId) submitBlockers.push("Change Type is required");
  if (!riskLevel) submitBlockers.push("Risk Level is required");
  if (!implementationDate) submitBlockers.push("Implementation Date is required");
  if (!impactDescription.trim()) submitBlockers.push("Impact Description is required");
  if (!rollbackPlan.trim()) submitBlockers.push("Backout Plan is required");

  const refreshRelatedData = async (changeId: string) => {
    if (!apiClient.isValidId(changeId)) return;
    const [nextApprovals, nextAttachments, nextTasks] = await Promise.all([
      apiClient.getApprovals(changeId),
      apiClient.getAttachments(changeId),
      apiClient.getTasks(changeId)
    ]);

    setApprovals(nextApprovals ?? []);
    setAttachments(nextAttachments ?? []);
    setTasks(nextTasks ?? []);
  };

  useEffect(() => {
    if (!id) return;
    if (!apiClient.isValidId(id)) {
      setError("Invalid change request id.");
      return;
    }

    setLoading(true);
    apiClient
      .getChangeById(id)
      .then((data) => {
        setItem(data);
        const descriptionBlob = data.description ?? "";
        setTitle(data.title ?? "");
        setDescription(data.description ?? "");
        setBusinessJustification(data.businessJustification ?? "");
        setImplementationSteps(data.implementationSteps ?? "");
        setBackoutPlan(data.backoutPlan ?? "");
        setService(data.serviceSystem ?? data.service ?? "");
        setCategory(data.category ?? "Application");
        setEnvironment(data.environment ?? "Non-Production");
        setDowntimeRequired(false);
        setChangeType(data.changeTypeId === 1 ? "Standard" : data.changeTypeId === 3 ? "Emergency" : "Normal");
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
        setLoading(false);
      });
  }, [id]);

  useEffect(() => {
    apiClient.getTemplates().then((items) => {
      if (items?.length) setTemplates(items.filter((x) => x.isActive));
    }).catch(() => void 0);
  }, []);

  const formIsDirty = Boolean(
    isNew ||
    !item ||
    title !== (item.title ?? "") ||
    description !== (item.description ?? "") ||
    implementationSteps !== (item.implementationSteps ?? "") ||
    backoutPlan !== (item.backoutPlan ?? "") ||
    service !== ((item.serviceSystem ?? item.service) ?? "") ||
    category !== (item.category ?? "Application") ||
    environment !== (item.environment ?? "Non-Production") ||
    businessJustification !== (item.businessJustification ?? "") ||
    changeTypeId !== (item.changeTypeId ?? 2) ||
    priority !== (item.priority ?? "P3") ||
    riskLevel !== (item.riskLevel ?? "Medium") ||
    impactLevel !== (item.impactLevel ?? "Medium") ||
    plannedStart !== (item.plannedStart ? item.plannedStart.slice(0, 16) : "") ||
    plannedEnd !== (item.plannedEnd ? item.plannedEnd.slice(0, 16) : "")
  );


  const applyTemplate = (tplId: string) => {
    setTemplateId(tplId);
    const tpl = templates.find((t) => t.templateId === tplId);
    if (!tpl) return;
    setCategory(tpl.category);
    if (!title) setTitle(tpl.name);
    setDescription(tpl.description);
    setImplementationSteps(tpl.implementationSteps ?? "");
    setBackoutPlan(tpl.backoutPlan ?? "");
    setService(tpl.serviceSystem ?? "");
    setEnvironment(tpl.environment ?? "Non-Production");
    setBusinessJustification(tpl.businessJustification ?? "");
  };

  const saveDraft = async (options?: { navigateOnCreate?: boolean }) => {
    setError(null);
    setLoading(true);
    try {
      if (isNew) {
        const payload: ChangeCreateDto = {
          title,
          description,
          implementationSteps,
          backoutPlan,
          serviceSystem: service,
          category,
          environment,
          businessJustification,
          changeTypeId,
          priority,
          priorityId: priorityToId(priority),
          riskLevel,
          riskLevelId: riskToId(riskLevel),
          impactLevel,
          plannedStart: plannedStart ? new Date(plannedStart).toISOString() : undefined,
          plannedEnd: plannedEnd ? new Date(plannedEnd).toISOString() : undefined
        };
        const created = await apiClient.createChange(payload);
        if (options?.navigateOnCreate ?? true) {
          nav(`/changes/${created.id}`);
        }
        return created.id;
      } else if (apiClient.isValidId(id)) {
        const payload: ChangeUpdateDto = {
          title,
          description,
          implementationSteps,
          backoutPlan,
          serviceSystem: service,
          category,
          environment,
          businessJustification,
          changeTypeId,
          priority,
          priorityId: priorityToId(priority),
          riskLevel,
          riskLevelId: riskToId(riskLevel),
          impactLevel,
          plannedStart: plannedStart ? new Date(plannedStart).toISOString() : undefined,
          plannedEnd: plannedEnd ? new Date(plannedEnd).toISOString() : undefined
        };
        const updated = await apiClient.updateChange(id, payload);
        setItem(updated);
        await refreshRelatedData(id);
        return id;
      }
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }

    return null;
  };

  const submitForApproval = async () => {
    setError(null);

    let targetId = id;
    if (formIsDirty) {
      targetId = await saveDraft({ navigateOnCreate: false });
      if (!apiClient.isValidId(targetId)) {
        return;
      }
    }

    if (!apiClient.isValidId(targetId)) {
      setError("Invalid change request id.");
      return;
    }

    setLoading(true);
    try {
      const submitted = await apiClient.submitChange(targetId);
      setItem(submitted);
      const refreshed = await apiClient.getChangeById(targetId);
      setItem(refreshed);
      await refreshRelatedData(targetId);
      if (isNew) {
        nav(`/changes/${targetId}`);
      }
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
    if (!apiClient.isValidId(id)) {
      setError("Invalid change request id.");
      return;
    }
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

  const addTask = async () => {
    if (!apiClient.isValidId(id) || !taskTitle.trim()) return;
    setLoading(true);
    setError(null);
    try {
      await apiClient.createTask(id, { title: taskTitle.trim(), description: taskDescription.trim(), statusId: 1 });
      setTaskTitle("");
      setTaskDescription("");
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
                <option key={t.templateId} value={t.templateId}>
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
                  <select className="select" value={changeType} onChange={(e) => setChangeType(e.target.value)}>
                    <option value="Normal">Normal</option>
                    <option value="Standard">Standard</option>
                    <option value="Emergency">Emergency</option>
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
            <button type="button" className="btn btn-primary" onClick={submitForApproval} disabled={isSubmitDisabled}>
              ✈ Submit for Approval
            </button>
          </div>
          {isSubmitDisabled && !loading && submitBlockers.length ? (
            <div className="small" style={{ marginTop: 8, color: "#fca5a5" }}>
              Missing required fields: {submitBlockers.join(", ")}
            </div>
          ) : null}
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
                <pre style={{ margin: 0, whiteSpace: "pre-wrap", fontFamily: "inherit" }}>{item?.implementationSteps ?? "—"}</pre>
              </div>
            </div>

            <div className="card">
              <div className="card-head">
                <div className="card-title">Backout Plan</div>
              </div>
              <div className="card-body">
                <pre style={{ margin: 0, whiteSpace: "pre-wrap", fontFamily: "inherit" }}>{item?.backoutPlan ?? "—"}</pre>
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
                <div className="h3">{item?.serviceSystem ?? item?.service ?? "—"}</div>
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
                    <div className="small">{fmtDT(approval.decisionAt)}</div>
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
          <div className="small">Allowed: pdf, doc(x), xls(x), png, jpg. Max 5 MB.</div>
          <input className="input" style={{ marginTop: 8 }} type="file" onChange={uploadAttachment} />
          <div style={{ display: "grid", gap: 8, marginTop: 12 }}>
            {attachments.map((attachment) => (
              <div key={attachment.id} className="row">
                <div className="row-left">
                  <div className="h3">{attachment.fileName}</div>
                  <div className="small">{Math.round(attachment.sizeBytes / 1024)} KB</div>
                </div>
                <a className="btn" href={`/api/changes/${id}/attachments/${attachment.id}/download`}>Download</a>
              </div>
            ))}
            {!attachments.length ? <div className="empty">No attachments uploaded.</div> : null}
          </div>
        </div>
      ) : null}

      {tab === "Tasks" ? (
        <div className="grid" style={{ gap: 12 }}>
          <div className="card card-pad">
            <div className="h3">Add Task</div>
            <div className="form-grid" style={{ marginTop: 8 }}>
              <div><input className="input" placeholder="Task title" value={taskTitle} onChange={(e) => setTaskTitle(e.target.value)} /></div>
              <div><input className="input" placeholder="Task description" value={taskDescription} onChange={(e) => setTaskDescription(e.target.value)} /></div>
            </div>
            <div style={{ marginTop: 8 }}><button className="btn btn-primary" disabled={!taskTitle.trim() || loading} onClick={addTask}>Add Task</button></div>
          </div>
          <div className="card card-pad">
            <div className="h3">Task List</div>
            <div style={{ display: "grid", gap: 8, marginTop: 10 }}>
              {tasks.map((task) => (
                <div key={task.id} className="row">
                  <div className="row-left">
                    <div className="h3">{task.title}</div>
                    <div className="small">{task.description ?? "No description"}</div>
                  </div>
                  <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                    <span className={pillForStatus(task.status)}>{task.status ?? "Pending"}</span>
                    <span className="small">Due: {fmtDT(task.dueAt)}</span>
                  </div>
                </div>
              ))}
              {!tasks.length ? <div className="empty">No tasks yet.</div> : null}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};

export default ChangeDetailPage;
