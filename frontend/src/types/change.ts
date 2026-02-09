export type ChangeRequest = {
  id: string;
  title: string;
  description: string;
  status: string;
  priority?: string;
  riskLevel?: string;
  plannedStart?: string;
  plannedEnd?: string;
  createdAt: string;
  updatedAt?: string;
};

export type DashboardStats = {
  totalChanges: number;
  pendingApprovals: number;
  scheduledThisWeek: number;
  completedThisMonth: number;
};

export type ChangeCreateDto = {
  title: string;
  description: string;
  priority?: string;
  riskLevel?: string;
  plannedStart?: string;
  plannedEnd?: string;
};

export type ChangeUpdateDto = {
  title: string;
  description: string;
  status: string;
  priority?: string;
  riskLevel?: string;
  plannedStart?: string;
  plannedEnd?: string;
};

export type ApprovalStatus = "Pending" | "Approved" | "Rejected";

export type Approval = {
  id: string;
  changeRequestId: string;
  approver: string;
  status: ApprovalStatus;
  comment?: string;
  decisionAt?: string;
};
