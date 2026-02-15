using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IChangeService
{
    Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken);
    Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken);
}

public class ChangeService : IChangeService
{
    private readonly IChangeRepository _repository;
    private readonly IAuditService _audit;

    public ChangeService(IChangeRepository repository, IAuditService audit)
    {
        _repository = repository;
        _audit = audit;
    }

    public Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken) => _repository.GetAllAsync(cancellationToken);
    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(changeRequest, cancellationToken);
        await _audit.LogAsync(1, created.CreatedBy, "system@local", "cm", "ChangeRequest", created.ChangeRequestId, created.ChangeNumber.ToString(), "Create", created.Title, cancellationToken);
        return created;
    }

    public async Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var updated = await _repository.UpdateAsync(changeRequest, cancellationToken);
        if (updated is not null)
        {
            await _audit.LogAsync(2, updated.UpdatedBy ?? updated.CreatedBy, "system@local", "cm", "ChangeRequest", updated.ChangeRequestId, updated.ChangeNumber.ToString(), "Update", updated.Title, cancellationToken);
        }

        return updated;
    }
}
