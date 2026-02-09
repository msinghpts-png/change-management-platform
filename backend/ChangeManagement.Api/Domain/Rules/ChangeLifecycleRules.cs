using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Domain.Rules;

public static class ChangeLifecycleRules
{
    public static readonly IReadOnlyDictionary<ChangeStatus, ChangeStatus[]> AllowedTransitions =
        new Dictionary<ChangeStatus, ChangeStatus[]>
        {
            { ChangeStatus.Draft, new[] { ChangeStatus.PendingApproval, ChangeStatus.Cancelled } },
            { ChangeStatus.PendingApproval, new[] { ChangeStatus.Approved, ChangeStatus.Rejected } },
            { ChangeStatus.Approved, new[] { ChangeStatus.Scheduled, ChangeStatus.Cancelled } },
            { ChangeStatus.Scheduled, new[] { ChangeStatus.InImplementation, ChangeStatus.Cancelled } },
            { ChangeStatus.InImplementation, new[] { ChangeStatus.Completed, ChangeStatus.Failed } },
            { ChangeStatus.Completed, Array.Empty<ChangeStatus>() },
            { ChangeStatus.Failed, Array.Empty<ChangeStatus>() },
            { ChangeStatus.Rejected, Array.Empty<ChangeStatus>() },
            { ChangeStatus.Cancelled, Array.Empty<ChangeStatus>() }
        };
}
