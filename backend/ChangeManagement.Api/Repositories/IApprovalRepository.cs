using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IApprovalRepository
{
    Task<ChangeApproval> CreateAsync(ChangeApproval approval, CancellationToken cancellationToken);
    Task<List<ChangeApproval>> GetByChangeAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeApproval?> GetByIdAsync(Guid approvalId, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
