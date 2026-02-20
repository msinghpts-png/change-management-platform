using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IApprovalRepository
{
    Task<ChangeApprover> CreateAsync(ChangeApprover approval, CancellationToken cancellationToken);
    Task<List<ChangeApprover>> GetByChangeAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeApprover?> GetByIdAsync(Guid approvalId, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
