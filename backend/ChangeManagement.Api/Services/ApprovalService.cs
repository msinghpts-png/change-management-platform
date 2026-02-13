using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;
using ChangeManagement.Api.Domain.Rules;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IApprovalService
{
    ChangeApproval CreateApproval(ChangeApproval approval);
    IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId);
    ApprovalDecisionResult RecordDecision(Guid changeId, Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repository;
    private readonly IChangeRepository _changeRepository;
    private readonly IChangeStatusValidator _statusValidator;

    public ApprovalService(IApprovalRepository repository, IChangeRepository changeRepository, IChangeStatusValidator statusValidator)
    {
        _repository = repository;
        _changeRepository = changeRepository;
        _statusValidator = statusValidator;
    }

    public ChangeApproval CreateApproval(ChangeApproval approval)
    {
        return _repository.AddApproval(approval);
    }

    public IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId)
    {
        return _repository.GetApprovalsForChange(changeRequestId);
    }

    public ApprovalDecisionResult RecordDecision(Guid changeId, Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt)
    {
        var existingApproval = _repository.GetById(approvalId);
        if (existingApproval is null)
        {
            return ApprovalDecisionResult.Failure("Approval not found.");
        }

        if (existingApproval.ChangeRequestId != changeId)
        {
            return ApprovalDecisionResult.Failure("Approval does not belong to this change request.");
        }

        var change = _changeRepository.GetById(existingApproval.ChangeRequestId);
        if (change is null)
        {
            return ApprovalDecisionResult.Failure("Change request not found.");
        }

        if (change.Status != ChangeStatus.PendingApproval)
        {
            return ApprovalDecisionResult.Failure("Change request is not pending approval.");
        }

        var updatedApproval = _repository.UpdateDecision(approvalId, status, comment, decidedAt);
        if (updatedApproval is null)
        {
            return ApprovalDecisionResult.Failure("Approval not found.");
        }

        var approvals = _repository.GetApprovalsForChange(change.Id).ToList();
        var nextStatus = change.Status;

        if (ApprovalPolicy.IsRejected(approvals))
        {
            nextStatus = ChangeStatus.Rejected;
        }
        else if (ApprovalPolicy.IsApproved(approvals))
        {
            nextStatus = ChangeStatus.Approved;
        }

        if (!_statusValidator.CanTransition(change.Status, nextStatus))
        {
            return ApprovalDecisionResult.Failure("Invalid status transition.");
        }

        change.Status = nextStatus;
        change.UpdatedAt = DateTime.UtcNow;
        _changeRepository.Update(change);

        return ApprovalDecisionResult.Success(updatedApproval);
    }
}

public record ApprovalDecisionResult(ChangeApproval? Approval, string? Error)
{
    public static ApprovalDecisionResult Success(ChangeApproval approval) => new(approval, null);
    public static ApprovalDecisionResult Failure(string error) => new(null, error);
}
