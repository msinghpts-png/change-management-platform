using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly ChangeManagementDbContext _dbContext;
    public ApprovalRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public async Task<ChangeApproval> CreateAsync(ChangeApproval approval, CancellationToken cancellationToken)
    {
        _dbContext.ChangeApprovals.Add(approval);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(approval.ChangeApprovalId, cancellationToken) ?? approval;
    }

    public Task<List<ChangeApproval>> GetByChangeAsync(Guid changeRequestId, CancellationToken cancellationToken) =>
        _dbContext.ChangeApprovals
            .Include(a => a.ApproverUser)
            .Include(a => a.ApprovalStatus)
            .Where(a => a.ChangeRequestId == changeRequestId)
            .ToListAsync(cancellationToken);

    public Task<ChangeApproval?> GetByIdAsync(Guid approvalId, CancellationToken cancellationToken) =>
        _dbContext.ChangeApprovals
            .Include(a => a.ApproverUser)
            .Include(a => a.ApprovalStatus)
            .FirstOrDefaultAsync(a => a.ChangeApprovalId == approvalId, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
