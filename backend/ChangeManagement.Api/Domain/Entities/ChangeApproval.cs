namespace ChangeManagement.Api.Domain.Entities;

public class ChangeApproval
{
    public Guid ChangeApprovalId { get; set; }
    public Guid ChangeRequestId { get; set; }
    public Guid ApproverUserId { get; set; }
    public int ApprovalStatusId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Comments { get; set; } = string.Empty;

    public ChangeRequest? ChangeRequest { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public User? ApproverUser { get; set; }
}
