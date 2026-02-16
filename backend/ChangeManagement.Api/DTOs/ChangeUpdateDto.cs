using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.DTOs;

public class ChangeUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public ChangeType? ChangeType { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public DateTime? ImplementationDate { get; set; }
    public string? ImpactDescription { get; set; }
    public string? RollbackPlan { get; set; }
    public Guid? AssignedToUserId { get; set; }
}
