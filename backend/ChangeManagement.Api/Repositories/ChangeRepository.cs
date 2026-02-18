using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeRepository : IChangeRepository
{
    private readonly ChangeManagementDbContext _dbContext;

    public ChangeRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken) =>
        BaseQuery().Where(c => c.DeletedAt == null).AsNoTracking().ToListAsync(cancellationToken);

    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        BaseQuery().FirstOrDefaultAsync(c => c.ChangeRequestId == id && c.DeletedAt == null, cancellationToken);

    public async Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        _dbContext.ChangeRequests.Add(changeRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(changeRequest.ChangeRequestId, cancellationToken) ?? changeRequest;
    }

    public async Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ChangeRequests
            .Include(c => c.ChangeApprovers)
            .Include(c => c.ChangeApprovals)
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequest.ChangeRequestId && c.DeletedAt == null, cancellationToken);
        if (existing is null) return null;

        _dbContext.Entry(existing).CurrentValues.SetValues(changeRequest);

        ReconcileApprovers(existing, changeRequest);
        ReconcileApprovals(existing, changeRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(changeRequest.ChangeRequestId, cancellationToken);
    }

    private void ReconcileApprovers(ChangeRequest existing, ChangeRequest incoming)
    {
        var incomingByUser = incoming.ChangeApprovers.ToDictionary(a => a.ApproverUserId, a => a);

        var toRemove = existing.ChangeApprovers.Where(a => !incomingByUser.ContainsKey(a.ApproverUserId)).ToList();
        foreach (var item in toRemove) _dbContext.ChangeApprovers.Remove(item);

        foreach (var inc in incoming.ChangeApprovers)
        {
            var match = existing.ChangeApprovers.FirstOrDefault(a => a.ApproverUserId == inc.ApproverUserId);
            if (match is null)
            {
                existing.ChangeApprovers.Add(new ChangeApprover
                {
                    ChangeApproverId = inc.ChangeApproverId == Guid.Empty ? Guid.NewGuid() : inc.ChangeApproverId,
                    ChangeRequestId = existing.ChangeRequestId,
                    ApproverUserId = inc.ApproverUserId,
                    CreatedAt = inc.CreatedAt == default ? DateTime.UtcNow : inc.CreatedAt
                });
            }
            else
            {
                match.CreatedAt = inc.CreatedAt == default ? match.CreatedAt : inc.CreatedAt;
            }
        }
    }

    private void ReconcileApprovals(ChangeRequest existing, ChangeRequest incoming)
    {
        var incomingByUser = incoming.ChangeApprovals.ToDictionary(a => a.ApproverUserId, a => a);

        var toRemove = existing.ChangeApprovals.Where(a => !incomingByUser.ContainsKey(a.ApproverUserId)).ToList();
        foreach (var item in toRemove) _dbContext.ChangeApprovals.Remove(item);

        foreach (var inc in incoming.ChangeApprovals)
        {
            var match = existing.ChangeApprovals.FirstOrDefault(a => a.ApproverUserId == inc.ApproverUserId);
            if (match is null)
            {
                existing.ChangeApprovals.Add(new ChangeApproval
                {
                    ChangeApprovalId = inc.ChangeApprovalId == Guid.Empty ? Guid.NewGuid() : inc.ChangeApprovalId,
                    ChangeRequestId = existing.ChangeRequestId,
                    ApproverUserId = inc.ApproverUserId,
                    ApprovalStatusId = inc.ApprovalStatusId,
                    ApprovedAt = inc.ApprovedAt,
                    Comments = inc.Comments
                });
            }
            else
            {
                match.ApprovalStatusId = inc.ApprovalStatusId;
                match.ApprovedAt = inc.ApprovedAt;
                match.Comments = inc.Comments;
            }
        }
    }

    private IQueryable<ChangeRequest> BaseQuery() => _dbContext.ChangeRequests
        .Include(c => c.ChangeType)
        .Include(c => c.Priority)
        .Include(c => c.Status)
        .Include(c => c.RiskLevel)
        .Include(c => c.RequestedByUser)
        .Include(c => c.AssignedToUser)
        .Include(c => c.ChangeApprovals).ThenInclude(a => a.ApprovalStatus)
        .Include(c => c.ChangeApprovals).ThenInclude(a => a.ApproverUser)
        .Include(c => c.ChangeApprovers).ThenInclude(a => a.ApproverUser)
        .Include(c => c.ChangeAttachments)
        .Include(c => c.ChangeTasks);
}
