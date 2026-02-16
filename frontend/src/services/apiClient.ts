import type { AppUser, Attachment, AuditLog, ChangeCreateDto, ChangeRequest, ChangeTask, ChangeUpdateDto } from "../types/change";

const API_BASE_URL = "/api";

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
    throw new Error(`Request failed: ${response.status}`);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
};

export const apiClient = {
  getChanges: () => request<ChangeRequest[]>("/changes"),
  getChangeById: (id: string) => request<ChangeRequest>(`/changes/${id}`),
  createChange: (payload: ChangeCreateDto) => request<ChangeRequest>("/changes", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  }),
  updateChange: (id: string, payload: ChangeUpdateDto) => request<ChangeRequest>(`/changes/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  }),
  submitChange: (id: string) => request<ChangeRequest>(`/changes/${id}/submit`, { method: "POST" }),
  approveChange: (id: string) => request<ChangeRequest>(`/changes/${id}/approve`, { method: "POST" }),
  rejectChange: (id: string) => request<ChangeRequest>(`/changes/${id}/reject`, { method: "POST" }),
  startChange: (id: string) => request<ChangeRequest>(`/changes/${id}/start`, { method: "POST" }),
  completeChange: (id: string) => request<ChangeRequest>(`/changes/${id}/complete`, { method: "POST" }),
  addTask: (id: string, payload: { title: string; description: string; dueDate?: string }) => request<ChangeTask>(`/changes/${id}/tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  }),
  getTasks: (id: string) => request<ChangeTask[]>(`/changes/${id}/tasks`),
  getAttachments: (id: string) => request<Attachment[]>(`/changes/${id}/attachments`),
  uploadAttachment: async (id: string, file: File): Promise<Attachment> => {
    const form = new FormData();
    form.append("file", file);

    const response = await fetch(`${API_BASE_URL}/changes/${id}/attachments`, {
      method: "POST",
      body: form,
      headers: withAuth()
    });

    if (!response.ok) throw new Error(`Request failed: ${response.status}`);
    return response.json() as Promise<Attachment>;
  },

  getAuditLogs: () => request<AuditLog[]>("/admin/audit"),
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
  }),
  resetPassword: (id: string, newPassword: string) => request<{ message: string }>(`/admin/users/${id}/reset-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ newPassword })
  }),
  login: (upn: string, password: string) => request<{ token: string; user: AppUser }>("/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ upn, password })
  })
};
