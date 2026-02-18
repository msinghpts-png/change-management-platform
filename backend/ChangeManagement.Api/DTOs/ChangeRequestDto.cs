namespace ChangeManagement.Api.DTOs;

public class ChangeRequestDto
{
    public Guid Id { get; set; }
    public Guid ChangeRequestId { get; set; }
    public int ChangeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImplementationSteps { get; set; }
    public string? BackoutPlan { get; set; }
    public string? ServiceSystem { get; set; }
    public string? Category { get; set; }
    public string? Environment { get; set; }
    public string? BusinessJustification { get; set; }
    public int ChangeTypeId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }
    public int RiskLevelId { get; set; }
    public int? ImpactTypeId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? RiskLevel { get; set; }
    public string? ImpactLevel { get; set; }
    public string? RequestedBy { get; set; }
    public bool ApprovalRequired { get; set; }
    public string ApprovalStrategy { get; set; } = "Any";
    public List<Guid> ApproverUserIds { get; set; } = new();
    public List<ApprovalDecisionItemDto> Approvals { get; set; } = new();
    public string? Owner { get; set; }
    public string? RequestedByDisplay { get; set; }
    public string? Executor { get; set; }
    public string? ImplementationGroup { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
