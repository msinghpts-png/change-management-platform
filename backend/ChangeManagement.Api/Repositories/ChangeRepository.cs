using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeRepository : IChangeRepository
{
    private readonly ChangeManagementDbContext _dbContext;

    public ChangeRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken) =>
        BaseQuery().AsNoTracking().ToListAsync(cancellationToken);

    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        BaseQuery().FirstOrDefaultAsync(c => c.ChangeId == id, cancellationToken);

    public async Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        _dbContext.ChangeRequests.Add(changeRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(changeRequest.ChangeId, cancellationToken) ?? changeRequest;
    }

    public async Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ChangeRequests.FirstOrDefaultAsync(c => c.ChangeId == changeRequest.ChangeId, cancellationToken);
        if (existing is null) return null;

        _dbContext.Entry(existing).CurrentValues.SetValues(changeRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(changeRequest.ChangeId, cancellationToken);
    }

    private IQueryable<ChangeRequest> BaseQuery() => _dbContext.ChangeRequests
        .Include(c => c.CreatedByUser)
        .Include(c => c.AssignedToUser)
        .Include(c => c.ChangeApprovals)
        .Include(c => c.ChangeAttachments)
        .Include(c => c.ChangeTasks);
}
