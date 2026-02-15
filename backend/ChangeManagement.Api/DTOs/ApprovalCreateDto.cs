namespace ChangeManagement.Api.DTOs;

public class ApprovalCreateDto
{
    public Guid ApproverUserId { get; set; }
    public string Comments { get; set; } = string.Empty;
}
