using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IApprovalService
{
    ChangeApproval CreateApproval(ChangeApproval approval);
    IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId);
    ChangeApproval? RecordDecision(Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repository;

    public ApprovalService(IApprovalRepository repository)
    {
        _repository = repository;
    }

    public ChangeApproval CreateApproval(ChangeApproval approval)
    {
        return _repository.AddApproval(approval);
    }

    public IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId)
    {
        return _repository.GetApprovalsForChange(changeRequestId);
    }

    public ChangeApproval? RecordDecision(Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt)
    {
        return _repository.UpdateDecision(approvalId, status, comment, decidedAt);
    }
}
