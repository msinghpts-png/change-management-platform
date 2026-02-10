using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Domain.Entities;

public class ChangeRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChangeStatus Status { get; set; }
    public string? Priority { get; set; }
    public string? RiskLevel { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ChangeApproval> Approvals { get; set; } = new List<ChangeApproval>();
}
