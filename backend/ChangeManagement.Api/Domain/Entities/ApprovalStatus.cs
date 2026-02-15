namespace ChangeManagement.Api.Domain.Entities;

public class ApprovalStatus
{
    public int ApprovalStatusId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ChangeApproval> ChangeApprovals { get; set; } = new List<ChangeApproval>();
}
