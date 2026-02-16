using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface IAuditService
{
    Task LogAsync(Guid actorUserId, string action, string details, Guid? changeId, CancellationToken cancellationToken);
    Task<List<AuditLog>> ListAsync(CancellationToken cancellationToken);
}

public class AuditService : IAuditService
{
    private readonly ChangeManagementDbContext _dbContext;
    public AuditService(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public async Task LogAsync(Guid actorUserId, string action, string details, Guid? changeId, CancellationToken cancellationToken)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ChangeId = changeId,
            ActorUserId = actorUserId,
            Action = action,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<AuditLog>> ListAsync(CancellationToken cancellationToken) =>
        _dbContext.AuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
}
