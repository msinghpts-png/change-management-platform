using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeRepository : IChangeRepository
{
    private readonly ChangeManagementDbContext _dbContext;

    public ChangeRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken) =>
        _dbContext.ChangeRequests
                        .Include(c => c.ChangeType)
            .Include(c => c.Priority)
            .Include(c => c.Status)
            .Include(c => c.RiskLevel)
            .Include(c => c.RequestedByUser)
            .Include(c => c.AssignedToUser)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        BaseQuery().FirstOrDefaultAsync(c => c.ChangeRequestId == id, cancellationToken);

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
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequest.ChangeRequestId, cancellationToken);
        if (existing is null) return null;

        _dbContext.Entry(existing).CurrentValues.SetValues(changeRequest);
        ReconcileApprovers(existing, changeRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(changeRequest.ChangeRequestId, cancellationToken);
    }

    private void ReconcileApprovers(ChangeRequest existing, ChangeRequest incoming)
    {
        var incomingByUser = incoming.ChangeApprovers
            .GroupBy(a => a.ApproverUserId)
            .ToDictionary(g => g.Key, g => g.Last());

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
                    ApprovalStatus = string.IsNullOrWhiteSpace(inc.ApprovalStatus) ? "Pending" : inc.ApprovalStatus,
                    DecisionAt = inc.DecisionAt,
                    DecisionComments = inc.DecisionComments,
                    CreatedAt = inc.CreatedAt == default ? DateTime.UtcNow : inc.CreatedAt
                });
            }
            else
            {
                match.ApprovalStatus = string.IsNullOrWhiteSpace(inc.ApprovalStatus) ? match.ApprovalStatus : inc.ApprovalStatus;
                match.DecisionAt = inc.DecisionAt;
                match.DecisionComments = inc.DecisionComments;
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
        .Include(c => c.ChangeApprovers).ThenInclude(a => a.ApproverUser)
        .Include(c => c.ChangeAttachments)
        .Include(c => c.ChangeTasks);
}
