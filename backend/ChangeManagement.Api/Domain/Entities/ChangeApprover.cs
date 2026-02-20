namespace ChangeManagement.Api.Domain.Entities;

public class ChangeApprover
{
    public Guid ChangeApproverId { get; set; }
    public Guid ChangeRequestId { get; set; }
    public Guid ApproverUserId { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime? DecisionAt { get; set; }
    public string? DecisionComments { get; set; }

    public DateTime CreatedAt { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? ApproverUser { get; set; }
}
