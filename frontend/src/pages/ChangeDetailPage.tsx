import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { Approval, ApprovalStatus, Attachment, ChangeCreateDto, ChangeRequest, ChangeTask, ChangeTemplate, ChangeUpdateDto } from "../types/change";
import { labelForChangeType, pillForChangeType, pillForImpactLevel, pillForRiskLevel } from "../utils/trafficColors";
import { getStatusPillClass } from "../ui/pills";

type ViewTab = "Overview" | "Approvals" | "Tasks" | "Attachments";
type FormTab = "Basic Info" | "Schedule" | "Plans" | "Risk & Impact" | "Approvals" | "Attachments";

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
    isActive: true,
    changeTypeId: 1,
    riskLevelId: 1
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
    isActive: true,
    changeTypeId: 1,
    riskLevelId: 1
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
    isActive: true,
    changeTypeId: 2,
    riskLevelId: 2
  }
];

const priorityToId = (priority: string) => {
  const normalized = priority.trim().toUpperCase();
  if (normalized === "P1") return 4;
  if (normalized === "P2") return 3;
  if (normalized === "P4") return 1;
  return 2;
};

const riskIdToLabel = (riskLevelId?: number) => {
  if (riskLevelId === 1) return "Low";
  if (riskLevelId === 3) return "High";
  return "Medium";
};

const impactIdToLabel = (impactTypeId?: number) => {
  if (impactTypeId === 1) return "Low";
  if (impactTypeId === 3) return "High";
  return "Medium";
};

const ChangeDetailPage = () => {
  const nav = useNavigate();
  const { id } = useParams();

  const hasValidId = apiClient.isValidId(id);
  const isNew = !hasValidId;

  const [tab, setTab] = useState<ViewTab>("Overview");
  const [formTab, setFormTab] = useState<FormTab>("Basic Info");

  const [loading, setLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [item, setItem] = useState<ChangeRequest | null>(null);

  const [approvals, setApprovals] = useState<Approval[]>([]);
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [tasks, setTasks] = useState<ChangeTask[]>([]);
  const [taskTitle, setTaskTitle] = useState("");
  const [taskDescription, setTaskDescription] = useState("");
  const [approverEmail, setApproverEmail] = useState("");
  const [approvalRequired, setApprovalRequired] = useState(false);
  const [approvalStrategy, setApprovalStrategy] = useState<"Any" | "Majority" | "All">("Any");
  const [implementationGroup, setImplementationGroup] = useState("");
  const [approvalComment, setApprovalComment] = useState("");
  const [decisionComment, setDecisionComment] = useState("");

  const authUserRole = (() => {
    try {
      const raw = localStorage.getItem("authUser");
      const parsed = raw ? JSON.parse(raw) : null;
      return typeof parsed?.role === "string" ? parsed.role.toLowerCase() : "";
    } catch {
      return "";
    }
  })();

  // Form fields (kept in UI; backend DTO is smaller)
  const [title, setTitle] = useState("");
  const [category, setCategory] = useState("Application");
  const [environment, setEnvironment] = useState("Non-Production");
  const [service, setService] = useState("");
  const [description, setDescription] = useState("");
  const [businessJustification, setBusinessJustification] = useState("");
  const [implementationSteps, setImplementationSteps] = useState("");
  const [backoutPlan, setBackoutPlan] = useState("");
  const [implementationWindowNotes, setImplementationWindowNotes] = useState("");

  const [changeTypeIdValue, setChangeTypeIdValue] = useState(2);
  const [priority, setPriority] = useState("P3");
  const [riskLevelIdValue, setRiskLevelIdValue] = useState(2);
  const [impactTypeIdValue, setImpactTypeIdValue] = useState(2);

  const [plannedStart, setPlannedStart] = useState("");
  const [plannedEnd, setPlannedEnd] = useState("");
  const [downtimeRequired, setDowntimeRequired] = useState(false);
  const [stakeholdersNotified, setStakeholdersNotified] = useState(false);

  const [templateId, setTemplateId] = useState("");
  const [templates, setTemplates] = useState<ChangeTemplate[]>(fallbackTemplates);
  const [selectedAttachment, setSelectedAttachment] = useState<File | null>(null);
  const [uploadingAttachment, setUploadingAttachment] = useState(false);

  const changeTypeId = changeTypeIdValue;
  const riskLevel = riskIdToLabel(riskLevelIdValue);
  const impactLevel = impactIdToLabel(impactTypeIdValue);
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

    const load = async () => {
      setLoading(true);
      try {
        const data = await apiClient.getChangeById(id);
        setItem(data);
        setTitle(data.title ?? "");
        setDescription(data.description ?? "");
        setBusinessJustification(data.businessJustification ?? "");
        setImplementationSteps(data.implementationSteps ?? "");
        setBackoutPlan(data.backoutPlan ?? "");
        setService(data.serviceSystem ?? data.service ?? "");
        setCategory(data.category ?? "Application");
        setEnvironment(data.environment ?? "Non-Production");
        setDowntimeRequired(Boolean((data as any).downtimeRequired));
        setChangeTypeIdValue(data.changeTypeId ?? 2);
        setPriority(data.priority ?? "P3");
        setRiskLevelIdValue(data.riskLevelId ?? (data.riskLevel?.toLowerCase() === "low" ? 1 : data.riskLevel?.toLowerCase() === "high" ? 3 : 2));
        setImpactTypeIdValue(data.impactTypeId ?? (data.impactLevel?.toLowerCase() === "low" ? 1 : data.impactLevel?.toLowerCase() === "high" ? 3 : 2));
        setPlannedStart(data.plannedStart ? data.plannedStart.slice(0, 16) : "");
        setPlannedEnd(data.plannedEnd ? data.plannedEnd.slice(0, 16) : "");
        setApprovalRequired(Boolean(data.approvalRequired));
        setApprovalStrategy((data.approvalStrategy as "Any" | "Majority" | "All") ?? "Any");
        setImplementationGroup(data.implementationGroup ?? "");
        await refreshRelatedData(id);
      } catch (err) {
        setError((err as Error).message);
      } finally {
        setLoading(false);
      }
    };

    void load();
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
    riskLevelIdValue !== (item.riskLevelId ?? ((item.riskLevel ?? "Medium").toLowerCase() === "low" ? 1 : (item.riskLevel ?? "Medium").toLowerCase() === "high" ? 3 : 2)) ||
    impactTypeIdValue !== (item.impactTypeId ?? ((item.impactLevel ?? "Medium").toLowerCase() === "low" ? 1 : (item.impactLevel ?? "Medium").toLowerCase() === "high" ? 3 : 2)) ||
    plannedStart !== (item.plannedStart ? item.plannedStart.slice(0, 16) : "") ||
    plannedEnd !== (item.plannedEnd ? item.plannedEnd.slice(0, 16) : "")
  );


  const applyTemplate = (tplId: string) => {
    setTemplateId(tplId);
    const tpl = templates.find((t) => t.templateId === tplId);
    if (!tpl) return;
    setCategory(tpl.category ?? "Application");
    if (!title) setTitle(tpl.name);
    setDescription(tpl.description ?? "");
    setImplementationSteps(tpl.implementationSteps ?? "");
    setBackoutPlan(tpl.backoutPlan ?? "");
    setService(tpl.serviceSystem ?? "");
    setEnvironment(tpl.environment ?? "Non-Production");
    setBusinessJustification(tpl.businessJustification ?? "");
    if (tpl.changeTypeId) setChangeTypeIdValue(tpl.changeTypeId);
    if (tpl.riskLevelId) setRiskLevelIdValue(tpl.riskLevelId);
  };

  const saveDraft = async (options?: { navigateOnCreate?: boolean }) => {
    if (isSaving) return null;

    setError(null);
    setIsSaving(true);
    setLoading(true);
    try {
      if (isNew) {
        const authUserRaw = localStorage.getItem("authUser");
        const authUser = authUserRaw ? JSON.parse(authUserRaw) as { id?: string } : null;
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
          riskLevelId: riskLevelIdValue,
          impactLevel,
          impactTypeId: impactTypeIdValue,
          requestedByUserId: authUser?.id,
          plannedStart: plannedStart ? new Date(plannedStart).toISOString() : undefined,
          plannedEnd: plannedEnd ? new Date(plannedEnd).toISOString() : undefined,
          approvalRequired: changeTypeId !== 2 ? true : approvalRequired,
          approvalStrategy,
          implementationGroup
        };
        const created = await apiClient.createChange(payload);
        if (options?.navigateOnCreate ?? true) {
          nav("/changes?mine=true");
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
          riskLevelId: riskLevelIdValue,
          impactLevel,
          impactTypeId: impactTypeIdValue,
          plannedStart: plannedStart ? new Date(plannedStart).toISOString() : undefined,
          plannedEnd: plannedEnd ? new Date(plannedEnd).toISOString() : undefined,
          approvalRequired: changeTypeId !== 2 ? true : approvalRequired,
          approvalStrategy,
          implementationGroup
        };
        const updated = await apiClient.updateChange(id, payload);
        setItem(updated);
        await refreshRelatedData(id);
        if (options?.navigateOnCreate ?? true) {
          nav(`/changes/${updated.id}`);
        }
        return updated.id;
      }
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setIsSaving(false);
      setLoading(false);
    }

    return null;
  };

  const submitForApproval = async () => {
    setError(null);

    let targetId: string | null = id ?? null;
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
      const submitted = await apiClient.submitChange(targetId, { approvalStrategy });
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
      setIsSaving(false);
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
      setIsSaving(false);
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
      setIsSaving(false);
      setLoading(false);
    }
  };


  const handleBannerDecision = async (action: "approve" | "reject") => {
    if (!apiClient.isValidId(id)) {
      setError("Invalid change request id.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const updated = action === "approve"
        ? await apiClient.approveChange(id, decisionComment.trim() || "Approved")
        : await apiClient.rejectChange(id, decisionComment.trim() || "Rejected");
      setItem(updated);
      setDecisionComment("");
      await refreshRelatedData(id);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setIsSaving(false);
      setLoading(false);
    }
  };


  const handleBannerDecision = async (action: "approve" | "reject") => {
    if (!apiClient.isValidId(id)) {
      setError("Invalid change request id.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const updated = action === "approve"
        ? await apiClient.approveChange(id, decisionComment.trim() || "Approved")
        : await apiClient.rejectChange(id, decisionComment.trim() || "Rejected");
      setItem(updated);
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
      setIsSaving(false);
      setLoading(false);
    }
  };

  const downloadAttachment = async (attachmentId: string, fileName: string) => {
    if (!apiClient.isValidId(id)) {
      setError("Invalid change request id.");
      return;
    }

    try {
      await apiClient.downloadAttachment(id, attachmentId, fileName);
    } catch (e) {
      setError((e as Error).message);
    }
  };

  const uploadAttachment = async () => {
    if (!selectedAttachment) return;
    setUploadingAttachment(true);
    setError(null);
    try {
      let targetId: string | null = id ?? null;
      if (!apiClient.isValidId(targetId)) {
        targetId = await saveDraft({ navigateOnCreate: false });
      }
      if (!apiClient.isValidId(targetId)) {
        setError("Save draft first before uploading.");
        return;
      }
      await apiClient.uploadAttachment(targetId, selectedAttachment);
      await refreshRelatedData(targetId);
      setSelectedAttachment(null);
      if (isNew) {
        nav(`/changes/${targetId}`);
      }
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setUploadingAttachment(false);
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
              {(["Basic Info", "Schedule", "Plans", "Risk & Impact", "Approvals", "Attachments"] as FormTab[]).map((t) => (
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
                  <div className="label">Change Type * <span title="Normal: DR / non-production / recurring monthly patching
Standard: requires CAB approval; scheduled change
Emergency: urgent; CAB approval required" style={{ cursor: "help" }}>ⓘ</span></div>
                  <select className="select" value={changeTypeIdValue} onChange={(e) => setChangeTypeIdValue(Number(e.target.value))}>
                    <option value={2}>Normal</option>
                    <option value={1}>Standard</option>
                    <option value={3}>Emergency</option>
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
                  <textarea className="textarea" value={implementationWindowNotes} onChange={(e) => setImplementationWindowNotes(e.target.value)} placeholder="Why is this change needed? What business value does it provide?" />
                </div>
              </div>
            ) : null}

            {formTab === "Schedule" ? (
              <div className="form-grid">
                <div>
                  <div className="label">Change Owner</div>
                  <input className="input" value={item?.owner ?? item?.requestedBy ?? ""} readOnly />
                </div>
                <div>
                  <div className="label">Implementation Group</div>
                  <input className="input" value={implementationGroup} onChange={(e) => setImplementationGroup(e.target.value)} placeholder="Team or group name" />
                </div>
                <div>
                  <div className="label">Planned Start *</div>
                  <input className="input" type="datetime-local" value={plannedStart} onChange={(e) => setPlannedStart(e.target.value)} />
                </div>
                <div>
                  <div className="label">Planned End *</div>
                  <input className="input" type="datetime-local" value={plannedEnd} onChange={(e) => setPlannedEnd(e.target.value)} />
                </div>
                <div style={{ gridColumn: "1 / -1" }}>
                  <div className="label">Implementation Window Notes</div>
                  <textarea className="textarea" value={implementationWindowNotes} onChange={(e) => setImplementationWindowNotes(e.target.value)} placeholder="Any specific timing requirements, maintenance windows, etc." />
                </div>
                <div className="switch-card">
                  <div>
                    <div className="h3">Downtime Required</div>
                    <div className="small">Will this change cause service interruption?</div>
                  </div>
                  <label className="switch">
                    <input type="checkbox" checked={downtimeRequired} onChange={(e) => setDowntimeRequired(e.target.checked)} />
                    <span className="switch-slider" />
                  </label>
                </div>
                <div className="switch-card">
                  <div>
                    <div className="h3">Stakeholders Notified</div>
                    <div className="small">Have affected parties been informed?</div>
                  </div>
                  <label className="switch">
                    <input type="checkbox" checked={stakeholdersNotified} onChange={(e) => setStakeholdersNotified(e.target.checked)} />
                    <span className="switch-slider" />
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
                    <option value="P1">P1 - Critical</option>
                    <option value="P2">P2 - High</option>
                    <option value="P3">P3 - Medium</option>
                    <option value="P4">P4 - Low</option>
                  </select>
                </div>
                <div>
                  <div className="label">Risk</div>
                  <select className="select" value={riskLevelIdValue} onChange={(e) => setRiskLevelIdValue(Number(e.target.value))}>
                    <option value={1}>Low</option>
                    <option value={2}>Medium</option>
                    <option value={3}>High</option>
                  </select>
                </div>
                <div>
                  <div className="label">Impact</div>
                  <select className="select" value={impactTypeIdValue} onChange={(e) => setImpactTypeIdValue(Number(e.target.value))}>
                    <option value={1}>Low</option>
                    <option value={2}>Medium</option>
                    <option value={3}>High</option>
                  </select>
                </div>
                <div />
              </div>
            ) : null}

            {formTab === "Approvals" ? (
              <div className="form-grid">
                <div>
                  <div className="label">Approval Required</div>
                  <label style={{ display: "flex", alignItems: "center", gap: 10 }}>
                    <input type="checkbox" checked={changeTypeId !== 2 ? true : approvalRequired} disabled={changeTypeId !== 2} onChange={(e) => setApprovalRequired(e.target.checked)} />
                    {changeTypeId !== 2 ? "Required for Standard/Emergency" : "Require CAB approval"}
                  </label>
                </div>
                <div>
                  <div className="label">Approval Strategy</div>
                  <select className="select" value={approvalStrategy} onChange={(e) => setApprovalStrategy(e.target.value as "Any" | "Majority" | "All")}>
                    <option value="Any">Any</option>
                    <option value="Majority">Majority</option>
                    <option value="All">All</option>
                  </select>
                </div>
              </div>
            ) : null}

            {formTab === "Attachments" ? (
              <div>
                <div className="h3">Attachments</div>
                <div className="small">Allowed: pdf, doc(x), xls(x), png, jpg. Max 5 MB.</div>
                <input className="input" style={{ marginTop: 8 }} type="file" onChange={(e) => setSelectedAttachment(e.target.files?.[0] ?? null)} />
                <div style={{ marginTop: 8 }}>
                  <button className="btn btn-primary" disabled={!selectedAttachment || uploadingAttachment} onClick={uploadAttachment}>
                    {uploadingAttachment ? "Uploading..." : "Upload Attachment"}
                  </button>
                </div>
                <div style={{ display: "grid", gap: 8, marginTop: 12 }}>
                  {attachments.map((attachment) => (
                    <div key={attachment.id} className="row">
                      <div className="row-left">
                        <div className="h3">{attachment.fileName}</div>
                        <div className="small">{Math.round(attachment.sizeBytes / 1024)} KB</div>
                      </div>
                      <button type="button" className="btn" onClick={() => { void downloadAttachment(attachment.id, attachment.fileName); }}>Download</button>
                    </div>
                  ))}
                  {!attachments.length ? <div className="empty">No attachments uploaded.</div> : null}
                </div>
              </div>
            ) : null}
          </div>

          <div className="footer-actions">
            <button className="btn" onClick={() => { void saveDraft(); }} disabled={loading || isSaving}>
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
            <span className={getStatusPillClass(item?.status)}>{item?.status ?? "—"}</span>
          </div>

          <h1 className="page-title" style={{ marginTop: 10 }}>{item?.title}</h1>

          <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginTop: 10 }}>
            <span className={pillForPriority(item?.priority)}>{item?.priority ?? "P3"}</span>
            <span className={pillForChangeType(item?.changeTypeId)}>{labelForChangeType(item?.changeTypeId)}</span>
            {item?.riskLevel ? <span className={pillForRiskLevel(item.riskLevel)}>Risk: {item.riskLevel}</span> : null}
            {item?.impactLevel ? <span className={pillForImpactLevel(item.impactLevel)}>Impact: {item.impactLevel}</span> : null}
          </div>
        </div>

        <button className="btn" onClick={() => alert("Implementation workflow can be wired next (status transition + audit).")}>
          ▶ Start Implementation
        </button>
      </div>


      {((item?.status ?? "").toLowerCase().includes("pendingapproval") || (item?.status ?? "").toLowerCase().includes("pending")) ? (
        <div className="card card-pad" style={{ background: "#fefce8", borderColor: "#facc15", marginBottom: 12, display: "flex", justifyContent: "space-between", alignItems: "center", gap: 12 }}>
          <div>
            <div className="h3">Approval Required</div>
            <div className="small">This change request is awaiting approval decision.</div>
          </div>
          {(["cab", "admin"].includes(authUserRole)) ? (
            <div style={{ display: "flex", gap: 8 }}>
              <button className="btn" type="button">Request Info</button>
              <button className="btn" type="button" disabled={loading} onClick={() => { void handleBannerDecision("reject"); }}>Reject</button>
              <button className="btn btn-primary" type="button" disabled={loading} onClick={() => { void handleBannerDecision("approve"); }}>Approve</button>
            </div>
          ) : (
            <div className="small">Approval is pending CAB/Admin review.</div>
          )}
        </div>
      ) : null}

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
                {downtimeRequired ? <div className="pill pill-amber">⚠ Downtime Required</div> : null}
              </div>
            </div>

            <div className="card">
              <div className="card-head">
                <div className="card-title">People</div>
              </div>
              <div className="card-body">
                <div className="small">Owner</div>
                <div className="h3">{item?.owner ?? item?.requestedBy ?? "—"}</div>
                <div style={{ height: 8 }} />
                <div className="small">Requested By</div>
                <div className="h3">{item?.requestedByDisplay ?? item?.requestedBy ?? "—"}</div>
                <div style={{ height: 8 }} />
                <div className="small">Executor</div>
                <div className="h3">{item?.executor ?? "—"}</div>
                <div style={{ height: 8 }} />
                <div className="small">Implementation Group</div>
                <div className="h3">{item?.implementationGroup ?? "—"}</div>
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
                    <span className={getStatusPillClass(approval.status)}>{approval.status}</span>
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
          <input className="input" style={{ marginTop: 8 }} type="file" onChange={(e) => setSelectedAttachment(e.target.files?.[0] ?? null)} />
            <div style={{ marginTop: 8 }}><button className="btn btn-primary" disabled={!selectedAttachment || uploadingAttachment} onClick={uploadAttachment}>{uploadingAttachment ? "Uploading..." : "Upload Attachment"}</button></div>
          <div style={{ display: "grid", gap: 8, marginTop: 12 }}>
            {attachments.map((attachment) => (
              <div key={attachment.id} className="row">
                <div className="row-left">
                  <div className="h3">{attachment.fileName}</div>
                  <div className="small">{Math.round(attachment.sizeBytes / 1024)} KB</div>
                </div>
                <button type="button" className="btn" onClick={() => { void downloadAttachment(attachment.id, attachment.fileName); }}>Download</button>
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
                    <span className={getStatusPillClass(task.status)}>{task.status ?? "Pending"}</span>
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
