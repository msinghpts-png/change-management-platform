namespace ChangeManagement.Api.Domain.Entities;

public class EventType
{
    public int EventTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<AuditEvent> Events { get; set; } = new List<AuditEvent>();
}
