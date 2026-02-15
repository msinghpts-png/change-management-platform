namespace ChangeManagement.Api.DTOs;

public class ChangeRequestDto
{
    public Guid ChangeRequestId { get; set; }
    public int ChangeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ChangeTypeId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }
    public int RiskLevelId { get; set; }
}
