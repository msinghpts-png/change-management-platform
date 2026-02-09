namespace ChangeManagement.Api.DTOs;

public class ChangeRequestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public string? RiskLevel { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ApprovalsTotal { get; set; }
    public int ApprovalsApproved { get; set; }
    public int ApprovalsRejected { get; set; }
    public int ApprovalsPending { get; set; }
}
