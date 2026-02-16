using System.Security.Claims;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuditService(ChangeManagementDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(int eventTypeId, Guid actorUserId, string actorUpn, string entitySchema, string entityName, Guid entityId, string changeNumber, string reason, string details, CancellationToken cancellationToken)
    {
        var resolvedActorUpn = actorUpn;
        if (string.IsNullOrWhiteSpace(resolvedActorUpn) || resolvedActorUpn.Equals("system@local", StringComparison.OrdinalIgnoreCase))
        {
            resolvedActorUpn = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
                ?? _httpContextAccessor.HttpContext?.User.Identity?.Name
                ?? "system@local";
        }

        _dbContext.AuditEvents.Add(new AuditEvent
        {
            AuditEventId = Guid.NewGuid(),
            EventTypeId = eventTypeId,
            EventAt = DateTime.UtcNow,
            ActorUserId = actorUserId,
            ActorUpn = resolvedActorUpn,
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
