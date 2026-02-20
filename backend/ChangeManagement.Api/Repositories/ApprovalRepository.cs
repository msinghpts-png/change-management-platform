using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly ChangeManagementDbContext _dbContext;
    public ApprovalRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public async Task<ChangeApprover> CreateAsync(ChangeApprover approval, CancellationToken cancellationToken)
    {
        _dbContext.ChangeApprovers.Add(approval);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(approval.ChangeApproverId, cancellationToken) ?? approval;
    }

    public Task<List<ChangeApprover>> GetByChangeAsync(Guid changeRequestId, CancellationToken cancellationToken) =>
        _dbContext.ChangeApprovers
            .Include(a => a.ApproverUser)
            .Where(a => a.ChangeRequestId == changeRequestId)
            .ToListAsync(cancellationToken);

    public Task<ChangeApprover?> GetByIdAsync(Guid approvalId, CancellationToken cancellationToken) =>
        _dbContext.ChangeApprovers
            .Include(a => a.ApproverUser)
            .FirstOrDefaultAsync(a => a.ChangeApproverId == approvalId, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
