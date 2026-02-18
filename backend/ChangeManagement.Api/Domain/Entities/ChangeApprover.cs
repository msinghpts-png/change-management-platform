namespace ChangeManagement.Api.Domain.Entities;

public class ChangeApprover
{
    public Guid ChangeApproverId { get; set; }
    public Guid ChangeId { get; set; }
    public Guid ApproverUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? ApproverUser { get; set; }
}
