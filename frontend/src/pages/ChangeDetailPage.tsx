import { useEffect, useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import { useParams } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { Approval, ApprovalStatus, ChangeCreateDto, ChangeRequest, ChangeUpdateDto } from "../types/change";

type ChangeFormState = {
  title: string;
  description: string;
  status: string;
  priority: string;
  riskLevel: string;
  plannedStart: string;
  plannedEnd: string;
};

const emptyForm: ChangeFormState = {
  title: "",
  description: "",
  status: "Draft",
  priority: "",
  riskLevel: "",
  plannedStart: "",
  plannedEnd: ""
};

const ChangeDetailPage = () => {
  const [change, setChange] = useState<ChangeRequest | null>(null);
  const [approvals, setApprovals] = useState<Approval[]>([]);
  const [formState, setFormState] = useState<ChangeFormState>(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const [decisionComments, setDecisionComments] = useState<Record<string, string>>({});
  const { id } = useParams<{ id: string }>();
  const isNew = id === "new";

  const loadApprovals = async (changeId: string) => {
    const data = await apiClient.getApprovals(changeId);
    setApprovals(data);
  };

  useEffect(() => {
    if (!id) {
      setError("No change selected.");
      return;
    }

    if (isNew) {
      setChange(null);
      setApprovals([]);
      setFormState(emptyForm);
      return;
    }

    apiClient
      .getChangeById(id)
      .then((detail) => {
        setChange(detail);
        setFormState({
          title: detail.title,
          description: detail.description,
          status: detail.status,
          priority: detail.priority ?? "",
          riskLevel: detail.riskLevel ?? "",
          plannedStart: detail.plannedStart ?? "",
          plannedEnd: detail.plannedEnd ?? ""
        });
        return loadApprovals(id);
      })
      .catch((err: Error) => setError(err.message));
  }, [id, isNew]);

  const handleChange =
    (field: keyof ChangeFormState) => (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormState((prev) => ({ ...prev, [field]: event.target.value }));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);

    try {
      if (isNew) {
        const payload: ChangeCreateDto = {
          title: formState.title,
          description: formState.description,
          priority: formState.priority || undefined,
          riskLevel: formState.riskLevel || undefined,
          plannedStart: formState.plannedStart || undefined,
          plannedEnd: formState.plannedEnd || undefined
        };
        const created = await apiClient.createChange(payload);
        console.log("Created change", created);
        setChange(created);
      } else if (id) {
        const payload: ChangeUpdateDto = {
          title: formState.title,
          description: formState.description,
          status: formState.status,
          priority: formState.priority || undefined,
          riskLevel: formState.riskLevel || undefined,
          plannedStart: formState.plannedStart || undefined,
          plannedEnd: formState.plannedEnd || undefined
        };
        const updated = await apiClient.updateChange(id, payload);
        console.log("Updated change", updated);
        setChange(updated);
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : "Request failed.";
      setError(message);
    }
  };

  const handleDecisionComment = (approvalId: string) => (event: ChangeEvent<HTMLTextAreaElement>) => {
    setDecisionComments((prev) => ({ ...prev, [approvalId]: event.target.value }));
  };

  const handleDecision = async (approvalId: string, status: ApprovalStatus) => {
    if (!id) {
      return;
    }

    const comment = decisionComments[approvalId];
    const updated = await apiClient.decideApproval(id, approvalId, { status, comment });
    console.log("Updated approval", updated);
    await loadApprovals(id);
  };

  return (
    <section>
      <h2>{isNew ? "New Change" : "Change Detail"}</h2>
      {error ? <p>{error}</p> : null}
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="title">Title</label>
          <input id="title" value={formState.title} onChange={handleChange("title")} />
        </div>
        <div>
          <label htmlFor="description">Description</label>
          <textarea id="description" value={formState.description} onChange={handleChange("description")} />
        </div>
        {!isNew ? (
          <div>
            <label htmlFor="status">Status</label>
            <input id="status" value={formState.status} onChange={handleChange("status")} />
          </div>
        ) : null}
        <div>
          <label htmlFor="priority">Priority</label>
          <input id="priority" value={formState.priority} onChange={handleChange("priority")} />
        </div>
        <div>
          <label htmlFor="riskLevel">Risk Level</label>
          <input id="riskLevel" value={formState.riskLevel} onChange={handleChange("riskLevel")} />
        </div>
        <div>
          <label htmlFor="plannedStart">Planned Start</label>
          <input id="plannedStart" value={formState.plannedStart} onChange={handleChange("plannedStart")} />
        </div>
        <div>
          <label htmlFor="plannedEnd">Planned End</label>
          <input id="plannedEnd" value={formState.plannedEnd} onChange={handleChange("plannedEnd")} />
        </div>
        <button type="submit">{isNew ? "Create Change" : "Update Change"}</button>
      </form>
      {change && !isNew ? (
        <div>
          <p>Loaded change: {change.title}</p>
        </div>
      ) : null}
      {!isNew ? (
        <section>
          <h3>Approvals</h3>
          {approvals.length === 0 ? <p>No approvals yet.</p> : null}
          <ul>
            {approvals.map((approval) => (
              <li key={approval.id}>
                <p>Approver: {approval.approver}</p>
                <p>Status: {approval.status}</p>
                {approval.comment ? <p>Comment: {approval.comment}</p> : null}
                {approval.status === "Pending" ? (
                  <div>
                    <textarea
                      value={decisionComments[approval.id] ?? ""}
                      onChange={handleDecisionComment(approval.id)}
                      placeholder="Add a comment"
                    />
                    <div>
                      <button type="button" onClick={() => handleDecision(approval.id, "Approved")}>
                        Approve
                      </button>
                      <button type="button" onClick={() => handleDecision(approval.id, "Rejected")}>
                        Reject
                      </button>
                    </div>
                  </div>
                ) : null}
              </li>
            ))}
          </ul>
        </section>
      ) : null}
    </section>
  );
};

export default ChangeDetailPage;
