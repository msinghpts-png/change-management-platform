using System.ComponentModel.DataAnnotations;

namespace ChangeManagement.Api.DTOs;

public class SubmitChangeRequestDto
{
    public List<Guid> ApproverUserIds { get; set; } = new();
    [RegularExpression("^(Any|Majority|All)$", ErrorMessage = "ApprovalStrategy must be one of: Any, Majority, All.")]
    public string? ApprovalStrategy { get; set; }
    public string? Reason { get; set; }
}
