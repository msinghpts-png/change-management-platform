using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IApprovalService
{
    Task<ChangeApproval> CreateApprovalAsync(ChangeApproval approval, CancellationToken cancellationToken);
    Task<List<ChangeApproval>> GetApprovalsForChangeAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeApproval?> RecordDecisionAsync(Guid changeId, Guid approvalId, int approvalStatusId, string comments, CancellationToken cancellationToken);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repository;
    private readonly IChangeRepository _changeRepository;
    private readonly IAuditService _audit;

    public ApprovalService(IApprovalRepository repository, IChangeRepository changeRepository, IAuditService audit)
    {
        _repository = repository;
        _changeRepository = changeRepository;
        _audit = audit;
    }

    public Task<ChangeApproval> CreateApprovalAsync(ChangeApproval approval, CancellationToken cancellationToken) => _repository.CreateAsync(approval, cancellationToken);
    public Task<List<ChangeApproval>> GetApprovalsForChangeAsync(Guid changeRequestId, CancellationToken cancellationToken) => _repository.GetByChangeAsync(changeRequestId, cancellationToken);

    public async Task<ChangeApproval?> RecordDecisionAsync(Guid changeId, Guid approvalId, int approvalStatusId, string comments, CancellationToken cancellationToken)
    {
        var approval = await _repository.GetByIdAsync(approvalId, cancellationToken);
        if (approval is null || approval.ChangeRequestId != changeId)
        {
            return null;
        }

        approval.ApprovalStatusId = approvalStatusId;
        approval.Comments = comments;
        approval.ApprovedAt = DateTime.UtcNow;
        await _repository.SaveAsync(cancellationToken);

        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is not null)
        {
            await _audit.LogAsync(4, approval.ApproverUserId, "system@local", "cm", "ChangeApproval", approval.ChangeApprovalId, change.ChangeNumber.ToString(), "Decision", comments, cancellationToken);
        }

        return approval;
    }
}
