using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IApprovalService
{
    Task<ChangeApprover> CreateApprovalAsync(ChangeApprover approval, CancellationToken cancellationToken);
    Task<List<ChangeApprover>> GetApprovalsForChangeAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeApprover?> RecordDecisionAsync(Guid changeId, Guid approvalId, int approvalStatusId, string comments, CancellationToken cancellationToken);
    Task<ChangeApprover?> ApproveChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken);
    Task<ChangeApprover?> RejectChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repository;
    private readonly IChangeWorkflowService _workflowService;

    public ApprovalService(IApprovalRepository repository, IChangeWorkflowService workflowService)
    {
        _repository = repository;
        _workflowService = workflowService;
    }

    public Task<ChangeApprover> CreateApprovalAsync(ChangeApprover approval, CancellationToken cancellationToken)
    {
        approval.ApprovalStatus = string.IsNullOrWhiteSpace(approval.ApprovalStatus) ? "Pending" : approval.ApprovalStatus;
        if (approval.CreatedAt == default)
        {
            approval.CreatedAt = DateTime.UtcNow;
        }

        return _repository.CreateAsync(approval, cancellationToken);
    }

    public Task<List<ChangeApprover>> GetApprovalsForChangeAsync(Guid changeRequestId, CancellationToken cancellationToken)
        => _repository.GetByChangeAsync(changeRequestId, cancellationToken);

    public async Task<ChangeApprover?> RecordDecisionAsync(Guid changeId, Guid approvalId, int approvalStatusId, string comments, CancellationToken cancellationToken)
    {
        var approval = await _repository.GetByIdAsync(approvalId, cancellationToken);
        if (approval is null || approval.ChangeRequestId != changeId)
        {
            return null;
        }

        if (approvalStatusId == 2)
        {
            await _workflowService.ApproveAsync(changeId, approval.ApproverUserId, comments, cancellationToken);
        }
        else if (approvalStatusId == 3)
        {
            await _workflowService.RejectAsync(changeId, approval.ApproverUserId, comments, cancellationToken);
        }
        else
        {
            return null;
        }

        return await _repository.GetByIdAsync(approvalId, cancellationToken);
    }

    public async Task<ChangeApprover?> ApproveChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken)
    {
        var approval = await EnsureApproverAsync(changeId, cabUserId, cancellationToken);
        if (approval is null)
        {
            return null;
        }

        return await RecordDecisionAsync(changeId, approval.ChangeApproverId, 2, comments, cancellationToken);
    }

    public async Task<ChangeApprover?> RejectChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken)
    {
        var approval = await EnsureApproverAsync(changeId, cabUserId, cancellationToken);
        if (approval is null)
        {
            return null;
        }

        return await RecordDecisionAsync(changeId, approval.ChangeApproverId, 3, comments, cancellationToken);
    }

    private async Task<ChangeApprover?> EnsureApproverAsync(Guid changeId, Guid cabUserId, CancellationToken cancellationToken)
    {
        var approvals = await _repository.GetByChangeAsync(changeId, cancellationToken);
        var approval = approvals.FirstOrDefault(a => a.ApproverUserId == cabUserId);
        if (approval is not null)
        {
            return approval;
        }

        return await _repository.CreateAsync(new ChangeApprover
        {
            ChangeApproverId = Guid.NewGuid(),
            ChangeRequestId = changeId,
            ApproverUserId = cabUserId,
            ApprovalStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }
}
