import type {
  Approval,
  ApprovalStatus,
  Attachment,
  ChangeCreateDto,
  ChangeRequest,
  ChangeUpdateDto,
  DashboardStats,
  DatabaseBackup,
  DatabaseStatus
} from "../types/change";

const API_BASE_URL = "/api";

const request = async <T>(path: string, options?: RequestInit): Promise<T> => {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return response.json() as Promise<T>;
};

export const apiClient = {
  getChanges: () => request<ChangeRequest[]>("/changes"),
  getChangeById: (id: string) => request<ChangeRequest>(`/changes/${id}`),
  getApprovals: (changeId: string) => request<Approval[]>(`/changes/${changeId}/approvals`),
  getAttachments: (changeId: string) => request<Attachment[]>(`/changes/${changeId}/attachments`),

  createChange: (payload: ChangeCreateDto) =>
    request<ChangeRequest>("/changes", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  createApproval: (changeId: string, payload: { approver: string; comment?: string }) =>
    request<Approval>(`/changes/${changeId}/approvals`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  submitChange: (changeId: string) =>
    request<ChangeRequest>(`/changes/${changeId}/submit`, {
      method: "POST"
    }),

  updateChange: (id: string, payload: ChangeUpdateDto) =>
    request<ChangeRequest>(`/changes/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  decideApproval: (changeId: string, approvalId: string, payload: { status: ApprovalStatus; comment?: string }) =>
    request<Approval>(`/changes/${changeId}/approvals/${approvalId}/decision`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  uploadAttachment: async (changeId: string, file: File): Promise<Attachment> => {
    const form = new FormData();
    form.append("file", file);

    const response = await fetch(`${API_BASE_URL}/changes/${changeId}/attachments`, {
      method: "POST",
      body: form
    });

    if (!response.ok) {
      throw new Error(`Request failed: ${response.status}`);
    }

    return (await response.json()) as Attachment;
  },

  deleteAttachment: async (changeId: string, attachmentId: string): Promise<void> => {
    const response = await fetch(`${API_BASE_URL}/changes/${changeId}/attachments/${attachmentId}`, {
      method: "DELETE"
    });

    if (!response.ok) {
      throw new Error(`Request failed: ${response.status}`);
    }
  },

  getDatabaseStatus: () => request<DatabaseStatus>("/admin/database/status"),
  runMigrations: () => request<{ message: string }>("/admin/database/migrate", { method: "POST" }),
  seedDatabase: () => request<{ seeded: boolean; message: string }>("/admin/database/seed", { method: "POST" }),

  exportDatabase: async (): Promise<Blob> => {
    const response = await fetch(`${API_BASE_URL}/admin/database/export`);
    if (!response.ok) {
      throw new Error(`Request failed: ${response.status}`);
    }

    return response.blob();
  },

  importDatabase: (payload: DatabaseBackup) =>
    request<{ message: string }>("/admin/database/import", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),

  getDashboardStats: () => request<DashboardStats>("/dashboard")
};
