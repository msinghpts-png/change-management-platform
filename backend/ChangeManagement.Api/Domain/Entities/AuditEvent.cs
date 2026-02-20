using System;

namespace ChangeManagement.Api.Domain.Entities;

public class AuditEvent
{
    public Guid AuditEventId { get; set; }
    public int EventTypeId { get; set; }

    public Guid? ActorUserId { get; set; }
    public string? ActorUpn { get; set; }

    public string? SchemaName { get; set; }
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }

    public string? EntityNumber { get; set; }
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    // ✅ RESTORE NAVIGATION PROPERTIES
    public EventType EventType { get; set; } = default!;
    public User? ActorUser { get; set; }
}
