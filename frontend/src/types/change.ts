export type ChangeType = "Standard" | "Emergency";
export type RiskLevel = "Low" | "Medium" | "High" | "Critical";
export type ChangeStatus = "Draft" | "Submitted" | "Approved" | "Rejected" | "InImplementation" | "Completed" | "Closed";

export type ChangeRequest = {
  changeId: string;
  changeNumber: number;
  title: string;
  description: string;
  changeType: ChangeType;
  riskLevel: RiskLevel;
  status: ChangeStatus;
  impactDescription: string;
  rollbackPlan: string;
  implementationDate?: string;
  implementationStartDate?: string;
  completedDate?: string;
  assignedToUserId?: string;
  createdAt: string;
  updatedAt: string;
};

export type ChangeCreateDto = {
  title: string;
  description: string;
  changeType?: ChangeType;
  riskLevel?: RiskLevel;
  implementationDate?: string;
  impactDescription: string;
  rollbackPlan: string;
  assignedToUserId?: string;
};

export type ChangeUpdateDto = Partial<ChangeCreateDto>;

export type ChangeTask = {
  changeTaskId: string;
  changeId: string;
  title: string;
  description: string;
  assignedToUserId?: string;
  dueDate?: string;
  completedDate?: string;
};

export type Attachment = {
  changeAttachmentId: string;
  changeId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
};

export type AuditLog = {
  auditLogId: string;
  changeId?: string;
  actorUserId: string;
  action: string;
  details: string;
  createdAt: string;
};

export type AppUser = {
  id: string;
  upn: string;
  displayName: string;
  role: string;
  isActive: boolean;
};
