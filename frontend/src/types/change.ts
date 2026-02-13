export type ApprovalStatus = "Pending" | "Approved" | "Rejected";

export type Approval = {
  id: string;
  changeRequestId: string;
  approver: string;
  status: ApprovalStatus;
  comment?: string;
  decisionAt?: string;
};

export type Attachment = {
  id: string;
  changeRequestId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: string;
};

export type DashboardStats = {
  totalChanges: number;
  pendingApprovals: number;
  scheduledThisWeek: number;
  completedThisMonth: number;
  inImplementation?: number;
  emergencyChanges?: number;
};

export type DatabaseStatus = {
  databaseName: string;
  totalChanges: number;
  totalApprovals: number;
  totalAttachments: number;
  hasPendingMigrations: boolean;
  pendingMigrations: string[];
};

export type DatabaseBackup = {
  changeRequests: ChangeRequest[];
  changeApprovals: Approval[];
  changeAttachments: Array<Attachment & { storagePath: string; contentBase64?: string }>;
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
  riskLevel?: RiskLevel;
  impactLevel?: ImpactLevel;
  category?: string;
  environment?: string;
  service?: string;
  requestedBy?: string;
  plannedStart?: string;
  plannedEnd?: string;
  createdAt?: string;
  updatedAt?: string;
  approvalsTotal?: number;
  approvalsApproved?: number;
  approvalsRejected?: number;
  approvalsPending?: number;
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
