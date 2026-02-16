import type { ChangeEvent } from "react";
import "./ApprovalDecisionForm.css";

type ApprovalDecisionFormProps = {
  comment: string;
  canAct: boolean;
  onCommentChange: (value: string) => void;
  onApprove: () => void;
  onReject: () => void;
};

const ApprovalDecisionForm = ({
  comment,
  canAct,
  onCommentChange,
  onApprove,
  onReject
}: ApprovalDecisionFormProps) => {
  return (
    <section className="approval-decision-form card">
      <h3>Approval Decision</h3>
      <label className="approval-label" htmlFor="decision-comment">
        Comment
      </label>
      <textarea
        id="decision-comment"
        className="textarea"
        value={comment}
        onChange={(event: ChangeEvent<HTMLTextAreaElement>) => onCommentChange(event.target.value)}
      />
      <div className="approval-actions">
        <button type="button" className="btn btn-secondary" disabled={!canAct} onClick={onReject}>
          Reject
        </button>
        <button type="button" className="btn btn-primary" disabled={!canAct} onClick={onApprove}>
          Approve
        </button>
      </div>
    </section>
  );
};

export default ApprovalDecisionForm;
