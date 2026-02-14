using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IChangeRepository
{
    Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken);
    Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken);
}
