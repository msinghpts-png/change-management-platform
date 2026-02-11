using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Domain.Entities;

public class ChangeApproval
{
    public Guid Id { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string Approver { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime? DecisionAt { get; set; }
}
