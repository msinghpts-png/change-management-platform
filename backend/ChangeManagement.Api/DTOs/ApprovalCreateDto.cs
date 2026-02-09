namespace ChangeManagement.Api.DTOs;

public class ApprovalCreateDto
{
    public string Approver { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
