namespace ChangeManagement.Api.Domain.Entities;

public class ChangeTemplate
{
    public Guid ChangeTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string ImpactDescription { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
