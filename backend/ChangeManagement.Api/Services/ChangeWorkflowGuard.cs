using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Services;

public static class ChangeWorkflowGuard
{
    public static bool CanEdit(ChangeRequest change) => change.Status < ChangeStatus.Approved;

    public static bool CanTransition(ChangeStatus from, ChangeStatus to)
    {
        return from switch
        {
            ChangeStatus.Draft => to == ChangeStatus.Submitted,
            ChangeStatus.Submitted => to == ChangeStatus.Approved || to == ChangeStatus.Rejected,
            ChangeStatus.Approved => to == ChangeStatus.InImplementation,
            ChangeStatus.InImplementation => to == ChangeStatus.Completed,
            ChangeStatus.Completed => to == ChangeStatus.Closed,
            _ => false
        };
    }
}
