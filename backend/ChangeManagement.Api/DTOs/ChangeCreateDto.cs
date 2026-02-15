namespace ChangeManagement.Api.DTOs;

public class ChangeCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ChangeTypeId { get; set; }
    public int PriorityId { get; set; }
    public int RiskLevelId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
}
