namespace ChangeManagement.Api.DTOs;

public class TransitionAuditDto
{
    public string Details { get; set; } = string.Empty;
    public object? TransitionDetails { get; set; }
}
