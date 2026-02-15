namespace ChangeManagement.Api.DTOs;

public class ApprovalDecisionDto
{
    public int ApprovalStatusId { get; set; }
    public string Comments { get; set; } = string.Empty;
}
