using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Domain.Rules;

public static class ApprovalPolicy
{
    public const int RequiredApprovals = 1;

    public static bool IsApproved(IEnumerable<ChangeApproval> approvals)
    {
        return approvals.Count(a => a.Status == ApprovalStatus.Approved) >= RequiredApprovals;
    }

    public static bool IsRejected(IEnumerable<ChangeApproval> approvals)
    {
        return approvals.Any(a => a.Status == ApprovalStatus.Rejected);
    }

    public static bool IsPending(IEnumerable<ChangeApproval> approvals)
    {
        return approvals.Any(a => a.Status == ApprovalStatus.Pending);
    }
}
