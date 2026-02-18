using System.ComponentModel.DataAnnotations;

namespace ChangeManagement.Api.DTOs;

public class ChangeUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
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
    public Guid? AssignedToUserId { get; set; }
    public bool? ApprovalRequired { get; set; }
    [RegularExpression("^(Any|Majority|All)$", ErrorMessage = "ApprovalStrategy must be one of: Any, Majority, All.")]
    public string? ApprovalStrategy { get; set; }
    public List<Guid>? ApproverUserIds { get; set; }
    public string? ImplementationGroup { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public Guid UpdatedBy { get; set; }
}
