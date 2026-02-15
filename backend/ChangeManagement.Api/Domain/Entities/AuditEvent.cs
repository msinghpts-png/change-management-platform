namespace ChangeManagement.Api.Domain.Entities;

public class AuditEvent
{
    public Guid AuditEventId { get; set; }
    public int EventTypeId { get; set; }
    public DateTime EventAt { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorUpn { get; set; } = string.Empty;
    public string EntitySchema { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;

    public EventType? EventType { get; set; }
}
