using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Repositories;

public interface IApprovalRepository
{
    ChangeApproval AddApproval(ChangeApproval approval);
    IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId);
    ChangeApproval? GetById(Guid approvalId);
    ChangeApproval? UpdateDecision(Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt);
}
