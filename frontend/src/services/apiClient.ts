import type {
  Approval,
  ApprovalStatus,
  ChangeCreateDto,
  ChangeRequest,
  ChangeUpdateDto,
  DashboardStats
} from "../types/change";

const API_BASE_URL = "http://localhost:8080";

const request = async <T>(path: string, options?: RequestInit): Promise<T> => {
  const response = await fetch(`${API_BASE_URL}${path}`, options);
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return response.json() as Promise<T>;
};

export const apiClient = {
  getChanges: () => request<ChangeRequest[]>("/api/changes"),
  getChangeById: (id: string) => request<ChangeRequest>(`/api/changes/${id}`),
  getApprovals: (changeId: string) =>
    request<Approval[]>(`/api/changes/${changeId}/approvals`),
  createChange: (payload: ChangeCreateDto) =>
    request<ChangeRequest>("/api/changes", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),
  createApproval: (changeId: string, payload: { approver: string; comment?: string }) =>
    request<Approval>(`/api/changes/${changeId}/approvals`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),
  submitChange: (changeId: string) =>
    request<ChangeRequest>(`/api/changes/${changeId}/submit`, {
      method: "POST"
    }),
  updateChange: (id: string, payload: ChangeUpdateDto) =>
    request<ChangeRequest>(`/api/changes/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),
  decideApproval: (changeId: string, approvalId: string, payload: { status: ApprovalStatus; comment?: string }) =>
    request<Approval>(`/api/changes/${changeId}/approvals/${approvalId}/decision`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    }),
  getDashboardStats: () => request<DashboardStats>("/api/dashboard")
};
