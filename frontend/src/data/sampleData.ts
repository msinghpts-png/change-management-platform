import type { Approval, ChangeRequest, DashboardStats } from "../types/change";

export const sampleDashboardStats: DashboardStats = {
  totalChanges: 24,
  pendingApprovals: 5,
  scheduledThisWeek: 8,
  completedThisMonth: 11,
  inImplementation: 4,
  emergencyChanges: 2
};

export const sampleChanges: ChangeRequest[] = [
  {
    id: "chg-00001",
    title: "Deploy Q1 Security Patches to Production Web Servers",
    description: "Apply critical Microsoft security patches to web servers.",
    status: "Approved",
    priority: "P2",
    riskLevel: "Medium",
    plannedStart: "2026-01-27T09:00:00Z",
    plannedEnd: "2026-01-27T13:00:00Z",
    createdAt: "2026-01-20T10:30:00Z",
    approvalsTotal: 2,
    approvalsApproved: 2,
    approvalsRejected: 0,
    approvalsPending: 0
  },
  {
    id: "chg-00002",
    title: "Emergency Database Performance Fix",
    description: "Resolve query latency on customer orders database.",
    status: "PendingApproval",
    priority: "P1",
    riskLevel: "High",
    plannedStart: "2026-01-28T01:00:00Z",
    plannedEnd: "2026-01-28T03:00:00Z",
    createdAt: "2026-01-22T06:30:00Z",
    approvalsTotal: 1,
    approvalsApproved: 0,
    approvalsRejected: 0,
    approvalsPending: 1
  },
  {
    id: "chg-00003",
    title: "Add Firewall Rule for New Payment Gateway",
    description: "Allow outbound HTTPS to new payment processor endpoint.",
    status: "Draft",
    priority: "P3",
    riskLevel: "Low",
    plannedStart: "2026-01-29T15:00:00Z",
    plannedEnd: "2026-01-29T16:00:00Z",
    createdAt: "2026-01-24T15:15:00Z",
    approvalsTotal: 0,
    approvalsApproved: 0,
    approvalsRejected: 0,
    approvalsPending: 0
  }
];

export const sampleApprovals: Approval[] = [
  {
    id: "appr-1",
    changeRequestId: "chg-00002",
    approver: "manager@example.com",
    status: "Pending",
    comment: "Waiting for risk review"
  },
  {
    id: "appr-2",
    changeRequestId: "chg-00001",
    approver: "cab@example.com",
    status: "Approved",
    comment: "CAB approved",
    decisionAt: "2026-01-25T14:00:00Z"
  }
];
