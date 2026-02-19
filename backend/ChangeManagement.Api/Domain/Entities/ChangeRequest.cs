namespace ChangeManagement.Api.Domain.Entities;

public class ChangeRequest
{
    public Guid ChangeRequestId { get; set; }
    public int ChangeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ChangeTypeId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }
    public int RiskLevelId { get; set; }
    public int? ImpactTypeId { get; set; }
    public bool ApprovalRequired { get; set; }
    public string ApprovalStrategy { get; set; } = ApprovalStrategies.Any;
    public Guid? ApprovalRequesterUserId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public string? ImplementationGroup { get; set; }
    public int? ImpactLevelId { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public string? DeletedReason { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public string? ImplementationSteps { get; set; }
    public string? BackoutPlan { get; set; }
    public string? ServiceSystem { get; set; }
    public string? Category { get; set; }
    public string? Environment { get; set; }
    public string? BusinessJustification { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ChangeType? ChangeType { get; set; }
    public ChangePriority? Priority { get; set; }
    public ChangeStatus? Status { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public User? RequestedByUser { get; set; }
    public User? AssignedToUser { get; set; }
    public User? ApprovalRequesterUser { get; set; }
    public User? SubmittedByUser { get; set; }
    public User? DeletedByUser { get; set; }
    public RiskLevel? ImpactLevel { get; set; }

    public ICollection<ChangeTask> ChangeTasks { get; set; } = new List<ChangeTask>();
    public ICollection<ChangeApproval> ChangeApprovals { get; set; } = new List<ChangeApproval>();
    public ICollection<ChangeApprover> ChangeApprovers { get; set; } = new List<ChangeApprover>();
    public ICollection<ChangeAttachment> ChangeAttachments { get; set; } = new List<ChangeAttachment>();
}
