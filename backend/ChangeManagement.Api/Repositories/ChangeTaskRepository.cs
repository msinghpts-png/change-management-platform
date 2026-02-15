using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeTaskRepository : IChangeTaskRepository
{
    private readonly ChangeManagementDbContext _dbContext;
    public ChangeTaskRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<List<ChangeTask>> GetByChangeIdAsync(Guid changeRequestId, CancellationToken cancellationToken) =>
        _dbContext.ChangeTasks.Where(t => t.ChangeRequestId == changeRequestId).ToListAsync(cancellationToken);

    public Task<ChangeTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken) => _dbContext.ChangeTasks.FirstOrDefaultAsync(t => t.ChangeTaskId == taskId, cancellationToken);

    public async Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        _dbContext.ChangeTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<ChangeTask?> UpdateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        var existing = await GetByIdAsync(task.ChangeTaskId, cancellationToken);
        if (existing is null) return null;
        _dbContext.Entry(existing).CurrentValues.SetValues(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await GetByIdAsync(taskId, cancellationToken);
        if (task is null) return false;
        _dbContext.ChangeTasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
