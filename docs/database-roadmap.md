# Database Architecture & Workflow Roadmap

## Database Architecture Diagram

```mermaid
erDiagram
    ChangeRequests ||--o{ ChangeApprovals : has
    ChangeRequests ||--o{ ChangeAttachments : has

    ChangeRequests {
      uniqueidentifier Id PK
      nvarchar Title
      nvarchar Description
      int Status
      nvarchar Priority
      nvarchar RiskLevel
      datetime2 PlannedStart
      datetime2 PlannedEnd
      datetime2 CreatedAt
      datetime2 UpdatedAt
    }

    ChangeApprovals {
      uniqueidentifier Id PK
      uniqueidentifier ChangeRequestId FK
      nvarchar Approver
      int Status
      nvarchar Comment
      datetime2 DecisionAt
    }

    ChangeAttachments {
      uniqueidentifier Id PK
      uniqueidentifier ChangeRequestId FK
      nvarchar FileName
      nvarchar ContentType
      bigint FileSize
      nvarchar StoragePath
      datetime2 UploadedAt
    }
```

## Approval Workflow State Diagram

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> PendingApproval: Submit
    PendingApproval --> Approved: All approvals approved
    PendingApproval --> Rejected: Any approval rejected
    Approved --> InProgress: Start implementation
    InProgress --> Completed: Work done
    Completed --> Closed: Post-implementation review complete
    Rejected --> Draft: Rework
```

## Recommended Next Enhancements

1. Role-based authentication (Admin / Requestor / Approver)
2. JWT authentication for API access
3. Email notifications for submission/approval/rejection events
4. Audit logging (who/what/when for all mutating actions)
5. Soft delete + retention policy
6. Optimistic concurrency tokens on mutable entities
7. Background job queue for exports, notifications, and cleanup
