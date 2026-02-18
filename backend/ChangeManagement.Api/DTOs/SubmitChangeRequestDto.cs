namespace ChangeManagement.Api.DTOs;

public class SubmitChangeRequestDto
{
    public List<Guid> ApproverUserIds { get; set; } = new();
    public string? ApprovalStrategy { get; set; }
    public string? Reason { get; set; }
}
