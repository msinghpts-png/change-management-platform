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
    private readonly IChangeRepository _changeRepository;
    private readonly IAuditService _audit;

    public ChangeTaskService(IChangeTaskRepository repository, IChangeRepository changeRepository, IAuditService audit)
    {
        _repository = repository;
        _changeRepository = changeRepository;
        _audit = audit;
    }

    public Task<List<ChangeTask>> GetByChangeAsync(Guid changeId, CancellationToken cancellationToken) => _repository.GetByChangeIdAsync(changeId, cancellationToken);

    public async Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(task, cancellationToken);
        var change = await _changeRepository.GetByIdAsync(created.ChangeRequestId, cancellationToken);
        if (change is not null)
        {
            await _audit.LogAsync(2, created.AssignedToUserId ?? change.CreatedBy, "system@local", "cm", "ChangeTask", created.ChangeTaskId, change.ChangeNumber.ToString(), "CreateTask", created.Title, cancellationToken);
        }

        return created;
    }

    public async Task<ChangeTask?> UpdateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        var updated = await _repository.UpdateAsync(task, cancellationToken);
        if (updated is null) return null;

        var change = await _changeRepository.GetByIdAsync(updated.ChangeRequestId, cancellationToken);
        if (change is not null)
        {
            await _audit.LogAsync(2, updated.AssignedToUserId ?? change.CreatedBy, "system@local", "cm", "ChangeTask", updated.ChangeTaskId, change.ChangeNumber.ToString(), "UpdateTask", updated.Title, cancellationToken);
        }

        return updated;
    }

    public Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken) => _repository.DeleteAsync(taskId, cancellationToken);
}
