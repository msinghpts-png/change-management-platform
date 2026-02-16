namespace ChangeManagement.Api.Domain.Entities;

public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public Guid? ChangeId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? ActorUser { get; set; }
}
