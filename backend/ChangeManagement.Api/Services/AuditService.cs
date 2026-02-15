using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Services;

public interface IAuditService
{
    Task LogAsync(int eventTypeId, Guid actorUserId, string actorUpn, string entitySchema, string entityName, Guid entityId, string changeNumber, string reason, string details, CancellationToken cancellationToken);
}

public class AuditService : IAuditService
{
    private readonly ChangeManagementDbContext _dbContext;
    public AuditService(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public async Task LogAsync(int eventTypeId, Guid actorUserId, string actorUpn, string entitySchema, string entityName, Guid entityId, string changeNumber, string reason, string details, CancellationToken cancellationToken)
    {
        _dbContext.AuditEvents.Add(new AuditEvent
        {
            AuditEventId = Guid.NewGuid(),
            EventTypeId = eventTypeId,
            EventAt = DateTime.UtcNow,
            ActorUserId = actorUserId,
            ActorUpn = actorUpn,
            EntitySchema = entitySchema,
            EntityName = entityName,
            EntityId = entityId,
            ChangeNumber = changeNumber,
            Reason = reason,
            Details = details
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
