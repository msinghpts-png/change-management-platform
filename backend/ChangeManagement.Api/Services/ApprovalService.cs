using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IApprovalService
{
    Task<ChangeApproval> CreateDecisionAsync(Guid changeId, bool isApproved, string comments, Guid actorUserId, CancellationToken cancellationToken);
    Task<List<ChangeApproval>> GetApprovalsForChangeAsync(Guid changeId, CancellationToken cancellationToken);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repository;

    public ApprovalService(IApprovalRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ChangeApproval>> GetApprovalsForChangeAsync(Guid changeId, CancellationToken cancellationToken) => _repository.GetByChangeAsync(changeId, cancellationToken);

    public Task<ChangeApproval> CreateDecisionAsync(Guid changeId, bool isApproved, string comments, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = new ChangeApproval
        {
            ChangeApprovalId = Guid.NewGuid(),
            ChangeId = changeId,
            CabUserId = actorUserId,
            IsApproved = isApproved,
            Comments = comments,
            DecisionDate = DateTime.UtcNow
        };

        return _repository.CreateAsync(entity, cancellationToken);
    }
}
