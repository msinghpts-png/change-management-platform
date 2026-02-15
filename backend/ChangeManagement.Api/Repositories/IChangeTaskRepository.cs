using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IChangeTaskRepository
{
    Task<List<ChangeTask>> GetByChangeIdAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken);
    Task<ChangeTask?> UpdateAsync(ChangeTask task, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken);
}
