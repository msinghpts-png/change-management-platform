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

export type ChangeTask = {
  id: string;
  changeRequestId: string;
  title: string;
  description?: string;
  status?: string;
  dueAt?: string;
  completedAt?: string;
};

export type AppUser = {
  id: string;
  upn: string;
  displayName: string;
  role: string;
  isActive: boolean;
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
  fileName: string;
  createdAt: string;
  sizeBytes: number;
};

export type DashboardStats = {
  totalChanges: number;
  pendingApprovals: number;
  scheduledThisWeek: number;
  completedThisMonth: number;
  inImplementation?: number;
  emergencyChanges?: number;
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
  implementationSteps?: string;
  backoutPlan?: string;
  serviceSystem?: string;
  category?: string;
  environment?: string;
  businessJustification?: string;
  changeTypeId?: number;
  status: ChangeRequestStatus;
  priority: ChangePriority;
  riskLevel?: RiskLevel;
  impactLevel?: ImpactLevel;
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
  description?: string;
  implementationSteps?: string;
  backoutPlan?: string;
  serviceSystem?: string;
  category?: string;
  environment?: string;
  businessJustification?: string;
  changeTypeId?: number;
  priorityId?: number;
  riskLevelId?: number;
  priority?: ChangePriority;
  riskLevel?: RiskLevel;
  impactLevel?: ImpactLevel;
  plannedStart?: string;
  plannedEnd?: string;
};

export type ChangeUpdateDto = {
  title?: string;
  description?: string;
  implementationSteps?: string;
  backoutPlan?: string;
  serviceSystem?: string;
  category?: string;
  environment?: string;
  businessJustification?: string;
  changeTypeId?: number;
  priorityId?: number;
  riskLevelId?: number;
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

export type ChangeTemplate = {
  templateId: string;
  name: string;
  description?: string;
  implementationSteps?: string;
  backoutPlan?: string;
  serviceSystem?: string;
  category?: string;
  environment?: string;
  businessJustification?: string;
  createdAt?: string;
  createdBy?: string;
  isActive: boolean;
};
