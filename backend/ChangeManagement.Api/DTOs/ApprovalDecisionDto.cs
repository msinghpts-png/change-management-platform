using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.DTOs;

public class ApprovalDecisionDto
{
    public ApprovalStatus Status { get; set; }
    public string? Comment { get; set; }
}
