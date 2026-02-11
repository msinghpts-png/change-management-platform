using ChangeManagement.Api.Domain.Enums;
using ChangeManagement.Api.Domain.Rules;

namespace ChangeManagement.Api.Services;

public interface IChangeStatusValidator
{
    bool CanTransition(ChangeStatus currentStatus, ChangeStatus nextStatus);
}

public class ChangeStatusValidator : IChangeStatusValidator
{
    public bool CanTransition(ChangeStatus currentStatus, ChangeStatus nextStatus)
    {
        if (!ChangeLifecycleRules.AllowedTransitions.TryGetValue(currentStatus, out var allowed))
        {
            return false;
        }

        return allowed.Contains(nextStatus);
    }
}
