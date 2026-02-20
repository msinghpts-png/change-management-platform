namespace ChangeManagement.Api.DTOs;

public class ApprovalDecisionItemDto
{
    public Guid Id { get; set; }
    public Guid ApproverUserId { get; set; }
    public string Approver { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? Comments { get; set; }
    public DateTime? DecisionAt { get; set; }
}
