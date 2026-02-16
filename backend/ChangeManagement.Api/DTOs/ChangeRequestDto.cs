using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.DTOs;

public class ChangeRequestDto
{
    public Guid ChangeId { get; set; }
    public int ChangeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public ChangeStatus Status { get; set; }
    public string ImpactDescription { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public DateTime? ImplementationDate { get; set; }
    public DateTime? ImplementationStartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
