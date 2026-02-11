import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import ApprovalDecisionForm from "../components/ApprovalDecisionForm";
import ApprovalList from "../components/ApprovalList";
import StatusBadge from "../components/StatusBadge";
import { sampleApprovals, sampleChanges } from "../data/sampleData";
import { apiClient } from "../services/apiClient";
import type { Approval, ApprovalStatus, ChangeCreateDto, ChangeRequest, ChangeUpdateDto } from "../types/change";
import "./ChangeDetailPage.css";

const defaultForm: ChangeCreateDto = {
  title: "",
  description: "",
  priority: "P3",
  riskLevel: "Low",
  plannedStart: "",
  plannedEnd: ""
};

const ChangeDetailPage = () => {
  const { id } = useParams();
  const isNew = !id || id === "new";
  const [activeSection, setActiveSection] = useState<"overview" | "approvals">("overview");
  const [change, setChange] = useState<ChangeRequest | null>(isNew ? null : sampleChanges[0]);
  const [approvals, setApprovals] = useState<Approval[]>([]);
  const [message, setMessage] = useState("");
  const [decisionComment, setDecisionComment] = useState("");
  const [formData, setFormData] = useState<ChangeCreateDto>(defaultForm);

  useEffect(() => {
    if (isNew || !id) {
      return;
    }

    apiClient
      .getChangeById(id)
      .then((loaded) => {
        setChange(loaded);
        setFormData({
          title: loaded.title,
          description: loaded.description,
          priority: loaded.priority,
          riskLevel: loaded.riskLevel,
          plannedStart: loaded.plannedStart,
          plannedEnd: loaded.plannedEnd
        });
      })
      .catch(() => {
        const fallback = sampleChanges.find((item) => item.id === id) ?? sampleChanges[0];
        setChange(fallback);
        setFormData({
          title: fallback.title,
          description: fallback.description,
          priority: fallback.priority,
          riskLevel: fallback.riskLevel,
          plannedStart: fallback.plannedStart,
          plannedEnd: fallback.plannedEnd
        });
      });

    apiClient
      .getApprovals(id)
      .then(setApprovals)
      .catch(() => setApprovals(sampleApprovals.filter((approval) => approval.changeRequestId === id)));
  }, [id, isNew]);

  const handleFieldChange = (field: keyof ChangeCreateDto, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    try {
      if (isNew) {
        await apiClient.createChange(formData);
        setMessage("Change request created successfully.");
        return;
      }

      if (!id || !change) {
        return;
      }

      const payload: ChangeUpdateDto = {
        ...formData,
        status: change.status
      };
      const updated = await apiClient.updateChange(id, payload);
      setChange(updated);
      setMessage("Change request updated successfully.");
    } catch {
      setMessage("Changes were captured in local mode.");
    }
  };

  const handleSubmitForApproval = async () => {
    if (!id) {
      return;
    }

    try {
      const updated = await apiClient.submitChange(id);
      setChange(updated);
    } catch {
      setChange((prev) => (prev ? { ...prev, status: "PendingApproval" } : prev));
    }
  };

  const handleDecision = async (status: ApprovalStatus) => {
    if (!id || approvals.length === 0) {
      return;
    }

    const target = approvals[0];

    try {
      const updated = await apiClient.decideApproval(id, target.id, { status, comment: decisionComment });
      setApprovals((prev) => prev.map((approval) => (approval.id === target.id ? updated : approval)));
    } catch {
      setApprovals((prev) =>
        prev.map((approval) =>
          approval.id === target.id
            ? { ...approval, status, comment: decisionComment, decisionAt: new Date().toISOString() }
            : approval
        )
      );
    }
  };

  return (
    <section>
      <div className="page-header">
        <div>
          <h2 className="page-title">{isNew ? "New Change Request" : "Change Detail"}</h2>
          <p className="page-subtitle">Track implementation readiness and approvals</p>
        </div>
        <Link className="btn btn-secondary" to="/changes">
          Back to list
        </Link>
      </div>

      {message ? <div className="change-detail-message">{message}</div> : null}

      <section className="card change-form-grid">
        <label>
          Title
          <input className="input" value={formData.title} onChange={(event) => handleFieldChange("title", event.target.value)} />
        </label>
        <label>
          Priority
          <select className="select" value={formData.priority ?? ""} onChange={(event) => handleFieldChange("priority", event.target.value)}>
            <option value="P1">P1</option>
            <option value="P2">P2</option>
            <option value="P3">P3</option>
            <option value="P4">P4</option>
          </select>
        </label>
        <label>
          Description
          <textarea
            className="textarea"
            value={formData.description}
            onChange={(event) => handleFieldChange("description", event.target.value)}
          />
        </label>
        <label>
          Risk Level
          <select className="select" value={formData.riskLevel ?? ""} onChange={(event) => handleFieldChange("riskLevel", event.target.value)}>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </label>
        <label>
          Planned Start
          <input
            className="input"
            type="datetime-local"
            value={formData.plannedStart ?? ""}
            onChange={(event) => handleFieldChange("plannedStart", event.target.value)}
          />
        </label>
        <label>
          Planned End
          <input
            className="input"
            type="datetime-local"
            value={formData.plannedEnd ?? ""}
            onChange={(event) => handleFieldChange("plannedEnd", event.target.value)}
          />
        </label>
      </section>

      <div className="change-detail-actions">
        <button type="button" className="btn btn-primary" onClick={handleSave}>
          {isNew ? "Create Change" : "Save Changes"}
        </button>
        {!isNew && change?.status === "Draft" ? (
          <button type="button" className="btn btn-secondary" onClick={handleSubmitForApproval}>
            Submit for approval
          </button>
        ) : null}
      </div>

      {!isNew && change ? (
        <section className="change-detail-body">
          <header className="card change-detail-meta">
            <div>
              <p className="change-detail-id">{change.id}</p>
              <h3 className="change-detail-name">{change.title}</h3>
            </div>
            <StatusBadge status={change.status} />
          </header>

          <div className="change-detail-tabs">
            <button
              type="button"
              className={`change-detail-tab ${activeSection === "overview" ? "active" : ""}`}
              onClick={() => setActiveSection("overview")}
            >
              Overview
            </button>
            <button
              type="button"
              className={`change-detail-tab ${activeSection === "approvals" ? "active" : ""}`}
              onClick={() => setActiveSection("approvals")}
            >
              Approvals
            </button>
          </div>

          {activeSection === "overview" ? (
            <article className="card">
              <h4 className="section-title">Change Description</h4>
              <p className="section-text">{change.description}</p>
            </article>
          ) : null}

          {activeSection === "approvals" ? (
            <div className="grid-2">
              <section className="card">
                <h4 className="section-title">Approval Records</h4>
                <ApprovalList approvals={approvals} />
              </section>
              <ApprovalDecisionForm
                comment={decisionComment}
                canAct={change.status === "PendingApproval" && approvals.length > 0}
                onCommentChange={setDecisionComment}
                onApprove={() => handleDecision("Approved")}
                onReject={() => handleDecision("Rejected")}
              />
            </div>
          ) : null}
        </section>
      ) : null}
    </section>
  );
};

export default ChangeDetailPage;
