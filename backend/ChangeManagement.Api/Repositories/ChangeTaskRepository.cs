using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeTaskRepository : IChangeTaskRepository
{
    private readonly ChangeManagementDbContext _dbContext;
    public ChangeTaskRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<List<ChangeTask>> GetByChangeIdAsync(Guid changeRequestId, CancellationToken cancellationToken) =>
        _dbContext.ChangeTasks
            .Include(t => t.AssignedToUser)
            .Include(t => t.Status)
            .Where(t => t.ChangeRequestId == changeRequestId)
            .ToListAsync(cancellationToken);

    public Task<ChangeTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken) => _dbContext.ChangeTasks
        .Include(t => t.AssignedToUser)
        .Include(t => t.Status)
        .FirstOrDefaultAsync(t => t.ChangeTaskId == taskId, cancellationToken);

    public async Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        _dbContext.ChangeTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(task.ChangeTaskId, cancellationToken) ?? task;
    }

    public async Task<ChangeTask?> UpdateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ChangeTasks.FirstOrDefaultAsync(x => x.ChangeTaskId == task.ChangeTaskId, cancellationToken);
        if (existing is null) return null;
        _dbContext.Entry(existing).CurrentValues.SetValues(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(task.ChangeTaskId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _dbContext.ChangeTasks.FirstOrDefaultAsync(x => x.ChangeTaskId == taskId, cancellationToken);
        if (task is null) return false;
        _dbContext.ChangeTasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
