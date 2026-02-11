import type {
  Approval,
  ApprovalStatus,
  ChangeCreateDto,
  ChangeRequest,
  ChangeUpdateDto,
  DashboardStats
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
  getApprovals: (changeId: string) =>
    request<Approval[]>(`/changes/${changeId}/approvals`),

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

  decideApproval: (
    changeId: string,
    approvalId: string,
    payload: { status: ApprovalStatus; comment?: string }
  ) =>
    request<Approval>(
      `/changes/${changeId}/approvals/${approvalId}/decision`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      }
    ),

  getDashboardStats: () => request<DashboardStats>("/dashboard")
};
