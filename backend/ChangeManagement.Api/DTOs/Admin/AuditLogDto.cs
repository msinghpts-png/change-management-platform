namespace ChangeManagement.Api.DTOs.Admin;

public class AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public Guid? ChangeId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
