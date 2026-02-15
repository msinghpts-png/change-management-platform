using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IChangeTaskService
{
    Task<List<ChangeTask>> GetByChangeAsync(Guid changeId, CancellationToken cancellationToken);
    Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken);
    Task<ChangeTask?> UpdateAsync(ChangeTask task, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken);
}

public class ChangeTaskService : IChangeTaskService
{
    private readonly IChangeTaskRepository _repository;
    public ChangeTaskService(IChangeTaskRepository repository) => _repository = repository;

    public Task<List<ChangeTask>> GetByChangeAsync(Guid changeId, CancellationToken cancellationToken) => _repository.GetByChangeIdAsync(changeId, cancellationToken);
    public Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken) => _repository.CreateAsync(task, cancellationToken);
    public Task<ChangeTask?> UpdateAsync(ChangeTask task, CancellationToken cancellationToken) => _repository.UpdateAsync(task, cancellationToken);
    public Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken) => _repository.DeleteAsync(taskId, cancellationToken);
}
