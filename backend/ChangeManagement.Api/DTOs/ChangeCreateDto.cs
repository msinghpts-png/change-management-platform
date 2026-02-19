using System.ComponentModel.DataAnnotations;

namespace ChangeManagement.Api.DTOs;

public class ChangeCreateDto
{
    public Guid? ChangeRequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImplementationSteps { get; set; }
    public string? BackoutPlan { get; set; }
    public string? ServiceSystem { get; set; }
    public string? Category { get; set; }
    public string? Environment { get; set; }
    public string? BusinessJustification { get; set; }

    // New DBML-aligned fields
    public int? ChangeTypeId { get; set; }
    public int? PriorityId { get; set; }
    public int? RiskLevelId { get; set; }
    public int? ImpactTypeId { get; set; }
    public int? ImpactLevelId { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public bool? ApprovalRequired { get; set; }
    [RegularExpression("^(Any|Majority|All)$", ErrorMessage = "ApprovalStrategy must be one of: Any, Majority, All.")]
    public string? ApprovalStrategy { get; set; }
    public List<Guid>? ApproverUserIds { get; set; }
    public string? ImplementationGroup { get; set; }
    public string? ImplementationWindowNotes { get; set; }
    public bool? DowntimeRequired { get; set; }
    public bool? StakeholdersNotified { get; set; }

    // Legacy UI compatibility fields
    public string? ChangeType { get; set; }
    public string? Priority { get; set; }
    public string? RiskLevel { get; set; }
    public string? RequestedBy { get; set; }

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
}
