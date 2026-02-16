using System.Security.Claims;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChangeTaskService(IChangeTaskRepository repository, IChangeRepository changeRepository, IAuditService audit, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _changeRepository = changeRepository;
        _audit = audit;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<List<ChangeTask>> GetByChangeAsync(Guid changeId, CancellationToken cancellationToken) => _repository.GetByChangeIdAsync(changeId, cancellationToken);

    public async Task<ChangeTask> CreateAsync(ChangeTask task, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(task, cancellationToken);
        var change = await _changeRepository.GetByIdAsync(created.ChangeRequestId, cancellationToken);
        if (change is not null)
        {
            await _audit.LogAsync(2, created.AssignedToUserId ?? change.CreatedBy, ResolveActorUpn(), "cm", "ChangeTask", created.ChangeTaskId, change.ChangeNumber.ToString(), "TaskCreate", created.Title, cancellationToken);
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
            await _audit.LogAsync(2, updated.AssignedToUserId ?? change.CreatedBy, ResolveActorUpn(), "cm", "ChangeTask", updated.ChangeTaskId, change.ChangeNumber.ToString(), "TaskUpdate", updated.Title, cancellationToken);
        }

        return updated;
    }

    private string ResolveActorUpn()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.Email)
               ?? user?.Identity?.Name
               ?? "system@local";
    }

    public Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken) => _repository.DeleteAsync(taskId, cancellationToken);
}
