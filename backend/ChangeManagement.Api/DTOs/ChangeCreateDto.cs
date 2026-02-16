namespace ChangeManagement.Api.DTOs;

public class ChangeCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // New DBML-aligned fields
    public int? ChangeTypeId { get; set; }
    public int? PriorityId { get; set; }
    public int? RiskLevelId { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    // Legacy UI compatibility fields
    public string? ChangeType { get; set; }
    public string? Priority { get; set; }
    public string? RiskLevel { get; set; }
    public string? RequestedBy { get; set; }

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
}
