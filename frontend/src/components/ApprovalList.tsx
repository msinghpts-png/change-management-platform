import type { Approval } from "../types/change";
import StatusBadge from "./StatusBadge";
import "./ApprovalList.css";

type ApprovalListProps = {
  approvals: Approval[];
};

const ApprovalList = ({ approvals }: ApprovalListProps) => {
  if (approvals.length === 0) {
    return <p className="approval-list-empty">No approval records available.</p>;
  }

  return (
    <ul className="approval-list">
      {approvals.map((approval) => (
        <li key={approval.id} className="approval-list-item">
          <div>
            <p className="approval-list-approver">{approval.approver}</p>
            <p className="approval-list-comment">{approval.comment || "No comment provided."}</p>
          </div>
          <StatusBadge status={approval.status} />
        </li>
      ))}
    </ul>
  );
};

export default ApprovalList;
