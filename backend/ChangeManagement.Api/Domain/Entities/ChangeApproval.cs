namespace ChangeManagement.Api.Domain.Entities;

public class ChangeApproval
{
    public Guid ChangeApprovalId { get; set; }
    public Guid ChangeId { get; set; }
    public Guid CabUserId { get; set; }
    public bool IsApproved { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? CabUser { get; set; }
}
