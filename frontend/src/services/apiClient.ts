import type {
  AppUser,
  Approval,
  ApprovalStatus,
  Attachment,
  ChangeCreateDto,
  ChangeRequest,
  ChangeTask,
  ChangeUpdateDto,
  DashboardStats,
  DatabaseBackup,
  DatabaseStatus,
  ChangeTemplate
} from "../types/change";

const API_BASE_URL = "/api";

const isValidId = (value?: string | null): value is string => Boolean(value && value !== "undefined" && value !== "null");

const withAuth = (headers?: HeadersInit): HeadersInit => {
  const token = localStorage.getItem("authToken");
  return token ? { ...headers, Authorization: `Bearer ${token}` } : headers ?? {};
};

const request = async <T>(path: string, options?: RequestInit): Promise<T> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: withAuth(options?.headers)
  });

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem("authToken");
      localStorage.removeItem("authUser");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
      throw new Error("Unauthorized. Please sign in again.");
    }

    throw new Error(`Request failed: ${response.status}`);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
};

const normalizeAttachment = (item: any): Attachment => ({
  id: item.id ?? item.changeAttachmentId,
  changeRequestId: item.changeRequestId,
  fileName: item.fileName,
  contentType: item.contentType ?? "application/octet-stream",
  sizeBytes: item.sizeBytes ?? item.fileSizeBytes ?? 0,
  uploadedAt: item.uploadedAt
});

const normalizeChange = (item: any): ChangeRequest => ({
  id: item.id ?? item.changeRequestId,
  changeNumber: item.changeNumber ? `CHG-${String(item.changeNumber).padStart(6, "0")}` : undefined,
  title: item.title,
  description: item.description,
  implementationSteps: item.implementationSteps,
  backoutPlan: item.backoutPlan,
  serviceSystem: item.serviceSystem,
  category: item.category,
  environment: item.environment,
  businessJustification: item.businessJustification,
  service: item.serviceSystem ?? item.service,
  changeTypeId: item.changeTypeId,
  riskLevelId: item.riskLevelId,
  priorityId: item.priorityId,
  statusId: item.statusId,
  status: item.status ?? "Draft",
  approvalRequired: item.approvalRequired,
  approvalStrategy: item.approvalStrategy ?? "Any",
  approverUserIds: item.approverUserIds ?? [],
  priority: item.priority ?? "P3",
  riskLevel: item.riskLevel,
  impactTypeId: item.impactTypeId,
  requestedBy: item.requestedBy,
  requestedByUserId: item.requestedByUserId,
  owner: item.owner,
  requestedByDisplay: item.requestedByDisplay,
  executor: item.executor,
  implementationGroup: item.implementationGroup,
  assignedToUserId: item.assignedToUserId,
  plannedStart: item.plannedStart,
  plannedEnd: item.plannedEnd,
  createdAt: item.createdAt,
  updatedAt: item.updatedAt
});

export const apiClient = {
  isValidId,
  getChanges: async (requestedByUserId?: string) => (await request<any[]>(`/changes${requestedByUserId ? `?requestedByUserId=${encodeURIComponent(requestedByUserId)}` : ""}`)).map(normalizeChange),
  getChangeById: async (id: string) => {
    if (!isValidId(id)) throw new Error("Invalid change id");
    return normalizeChange(await request<any>(`/changes/${id}`));
  },
  getApprovals: async (changeId: string) => {
    if (!isValidId(changeId)) return [] as Approval[];
    return request<Approval[]>(`/changes/${changeId}/approvals`);
  },
  getAttachments: async (changeId: string) => {
    if (!isValidId(changeId)) return [] as Attachment[];
    return (await request<any[]>(`/changes/${changeId}/attachments`)).map(normalizeAttachment);
  },
  getTasks: async (changeId: string) => {
    if (!isValidId(changeId)) return [] as ChangeTask[];
    return request<ChangeTask[]>(`/changes/${changeId}/tasks`);
  },

  getTemplates: () => request<ChangeTemplate[]>("/templates"),

  createTemplate: (payload: Partial<ChangeTemplate> & { name: string }) => request<ChangeTemplate>("/templates", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  updateTemplate: (id: string, payload: Partial<ChangeTemplate> & { name: string }) => request<ChangeTemplate>(`/templates/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  deleteTemplate: (id: string) => request<void>(`/templates/${id}`, { method: "DELETE" }),


  createTask: (changeId: string, payload: { title: string; description?: string; statusId?: number; assignedToUserId?: string; dueAt?: string }) =>
    request<ChangeTask>(`/changes/${changeId}/tasks`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  updateTask: (changeId: string, taskId: string, payload: { title: string; description?: string; statusId?: number; assignedToUserId?: string; dueAt?: string; completedAt?: string }) =>
    request<ChangeTask>(`/changes/${changeId}/tasks/${taskId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),
  createChange: async (payload: ChangeCreateDto) => normalizeChange(await request<any>("/changes", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    })),

  createApproval: (changeId: string, payload: { approver: string; comment?: string }) =>
    request<Approval>(`/changes/${changeId}/approvals`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ approver: payload.approver, comments: payload.comment ?? "" })
    }),

  submitChange: async (changeId: string, payload?: { approverUserIds?: string[]; approvalStrategy?: string; reason?: string }) => normalizeChange(await request<any>(`/changes/${changeId}/submit`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload ?? {})
    })),

  approveChange: (changeId: string, comments?: string) => request<any>(`/changes/${changeId}/approve`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ comments: comments ?? "" }) }).then(normalizeChange),
  rejectChange: (changeId: string, comments?: string) => request<any>(`/changes/${changeId}/reject`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ comments: comments ?? "" }) }).then(normalizeChange),
  revertToDraft: (changeId: string, reason?: string) => request<any>(`/changes/${changeId}/revert-to-draft`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ reason: reason ?? "" }) }).then(normalizeChange),
  startChange: (changeId: string) => request<any>(`/changes/${changeId}/start`, { method: "POST" }).then(normalizeChange),
  completeChange: (changeId: string) => request<any>(`/changes/${changeId}/complete`, { method: "POST" }).then(normalizeChange),
  closeChange: (changeId: string) => request<any>(`/changes/${changeId}/close`, { method: "POST" }).then(normalizeChange),
  cancelChange: (changeId: string, reason?: string) => request<any>(`/changes/${changeId}/cancel`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ reason: reason ?? "" }) }).then(normalizeChange),

  updateChange: async (id: string, payload: ChangeUpdateDto) => normalizeChange(await request<any>(`/changes/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    })),

  decideApproval: (changeId: string, approvalId: string, payload: { status: ApprovalStatus; comment?: string }) =>
    request<Approval>(`/changes/${changeId}/approvals/${approvalId}/decision`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ approvalStatusId: payload.status === "Approved" ? 2 : payload.status === "Rejected" ? 3 : 1, comments: payload.comment ?? "" })
    }),

  uploadAttachment: async (changeId: string, file: File): Promise<Attachment> => {
    const form = new FormData();
    form.append("file", file);

    const response = await fetch(`${API_BASE_URL}/changes/${changeId}/attachments`, {
      method: "POST",
      body: form,
      headers: withAuth()
    });

    if (!response.ok) {
      throw new Error(`Request failed: ${response.status}`);
    }

    return normalizeAttachment(await response.json());
  },

  downloadAttachment: async (changeId: string, attachmentId: string, fileName: string): Promise<void> => {
    const response = await fetch(`${API_BASE_URL}/changes/${changeId}/attachments/${attachmentId}/download`, {
      method: "GET",
      headers: withAuth()
    });

    if (!response.ok) {
      throw new Error(`Request failed: ${response.status}`);
    }

    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
  },

  deleteAttachment: async (changeId: string, attachmentId: string): Promise<void> => {
    await request<void>(`/changes/${changeId}/attachments/${attachmentId}`, { method: "DELETE" });
  },

  getDashboardStats: () => request<DashboardStats>("/dashboard"),
  getDatabaseStatus: () => request<DatabaseStatus>("/admin/database/status"),
  exportDatabase: () => request<DatabaseBackup[]>("/admin/database/backups"),
  importDatabase: (_file: File) => Promise.resolve(),
  runMigrations: () => request<{ message: string }>("/admin/database/migrate", { method: "POST" }),
  seedDatabase: () => request<{ message: string }>("/admin/demo-data", { method: "POST" }),

  getAuditEvents: () => request<any[]>("/admin/audit"),

  getAllAttachments: (changeNumber?: string) => request<any[]>(`/admin/attachments${changeNumber ? `?changeNumber=${encodeURIComponent(changeNumber)}` : ""}`),
  deleteAdminAttachment: (attachmentId: string) => request<void>(`/admin/attachments/${attachmentId}`, { method: "DELETE" }),

  resetUserPassword: (id: string, newPassword: string) => request<{ message: string }>(`/admin/users/${id}/reset-password`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ newPassword })
    }),

  login: (upn: string, password: string) => request<{ token: string; user: AppUser }>("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ upn, password })
    }),

  getUsers: () => request<AppUser[]>("/admin/users"),
  createUser: (payload: { upn: string; displayName: string; role: string; password: string }) => request<AppUser>("/admin/users", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),
  updateUser: (id: string, payload: { role: string; isActive: boolean }) => request<AppUser>(`/admin/users/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    })
};
