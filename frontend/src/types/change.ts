export type ApprovalStatus = "Pending" | "Approved" | "Rejected";

export type Approval = {
  id: string;
  changeId: string;
  approver: string;
  status: ApprovalStatus;
  comment?: string;
  decidedAt?: string;
};

export type DashboardStats = {
  totalChanges: number;
  pendingApprovals: number;
  scheduledThisWeek: number;
  completedThisMonth: number;
};

export type ChangeRequestStatus =
  | "Draft"
  | "PendingApproval"
  | "Approved"
  | "InProgress"
  | "Completed"
  | "Closed"
  | "Rejected"
  | string;

export type ChangePriority = "P1" | "P2" | "P3" | "P4" | string;
export type RiskLevel = "Low" | "Medium" | "High" | string;
export type ImpactLevel = "Low" | "Medium" | "High" | string;

export type ChangeRequest = {
  id: string;
  changeNumber?: string;

  title: string;
  description: string;

  status: ChangeRequestStatus;
  priority: ChangePriority;

  // kept for backward compatibility with backend naming
  riskLevel?: RiskLevel;
  impactLevel?: ImpactLevel;

  // Optional “enterprise UI” fields (backend can add later)
  category?: string;
  environment?: string;
  service?: string;
  requestedBy?: string;

  plannedStart?: string;
  plannedEnd?: string;
  createdAt?: string;
  updatedAt?: string;
};

export type ChangeCreateDto = {
  title: string;
  description: string;

  priority?: ChangePriority;
  riskLevel?: RiskLevel;
  impactLevel?: ImpactLevel;

  plannedStart?: string;
  plannedEnd?: string;
};

export type ChangeUpdateDto = {
  title?: string;
  description?: string;

  priority?: ChangePriority;
  riskLevel?: RiskLevel;
  impactLevel?: ImpactLevel;

  plannedStart?: string;
  plannedEnd?: string;

  status?: ChangeRequestStatus;
};

export type ApprovalDecisionDto = {
  status: ApprovalStatus;
  comment?: string;
};
