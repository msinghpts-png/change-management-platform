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

    public ICollection<ChangeTask> ChangeTasks { get; set; } = new List<ChangeTask>();
    public ICollection<ChangeApproval> ChangeApprovals { get; set; } = new List<ChangeApproval>();
    public ICollection<ChangeAttachment> ChangeAttachments { get; set; } = new List<ChangeAttachment>();
}
