using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly List<ChangeApproval> _approvals = new();

    public ChangeApproval AddApproval(ChangeApproval approval)
    {
        _approvals.Add(approval);
        return approval;
    }

    public IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId)
    {
        return _approvals.Where(approval => approval.ChangeRequestId == changeRequestId);
    }

    public ChangeApproval? GetById(Guid approvalId)
    {
        return _approvals.FirstOrDefault(item => item.Id == approvalId);
    }

    public ChangeApproval? UpdateDecision(Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt)
    {
        var approval = GetById(approvalId);
        if (approval is null)
        {
            return null;
        }

        approval.Status = status;
        approval.Comment = comment;
        approval.DecisionAt = decidedAt;

        return approval;
    }
}
