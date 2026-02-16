using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IChangeTaskService
{
    Task<List<ChangeTask>> GetByChangeAsync(Guid changeId, CancellationToken cancellationToken);
    Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken);
}

public class ChangeTaskService : IChangeTaskService
{
    private readonly IChangeTaskRepository _repository;
    private readonly IAuditService _audit;

    public ChangeTaskService(IChangeTaskRepository repository, IAuditService audit)
    {
        _repository = repository;
        _audit = audit;
    }

    public Task<List<ChangeTask>> GetByChangeAsync(Guid changeId, CancellationToken cancellationToken) => _repository.GetByChangeIdAsync(changeId, cancellationToken);

    public async Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(task, cancellationToken);
        await _audit.LogAsync(task.AssignedToUserId ?? Guid.Empty, "TaskCreated", task.Title, task.ChangeId, cancellationToken);
        return created;
    }
}
