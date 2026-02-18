using System.Security.Claims;
using System.Text.Json;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IChangeWorkflowService
{
    Task<ChangeRequest?> SubmitAsync(Guid changeId, Guid actorUserId, IReadOnlyCollection<Guid> approverUserIds, string? approvalStrategy, string? reason, CancellationToken cancellationToken);
    Task<ChangeRequest?> ApproveAsync(Guid changeId, Guid actorUserId, string? comments, CancellationToken cancellationToken);
    Task<ChangeRequest?> RejectAsync(Guid changeId, Guid actorUserId, string? comments, CancellationToken cancellationToken);
    Task<ChangeRequest?> RevertToDraftAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken);
    Task<ChangeRequest?> StartAsync(Guid changeId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ChangeRequest?> CompleteAsync(Guid changeId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ChangeRequest?> CloseAsync(Guid changeId, Guid actorUserId, CancellationToken cancellationToken);
    Task<ChangeRequest?> CancelAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken);
    Task<ChangeRequest?> SoftDeleteAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken);
}

public class ChangeWorkflowService : IChangeWorkflowService
{
    private const int Draft = 1;
    private const int Submitted = 2;
    private const int PendingApproval = 3;
    private const int Approved = 4;
    private const int Rejected = 5;
    private const int Scheduled = 6;
    private const int InImplementation = 7;
    private const int Completed = 8;
    private const int Closed = 9;
    private const int Cancelled = 10;

    private readonly IChangeRepository _changeRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly IAuditService _audit;
    private readonly IHttpContextAccessor _contextAccessor;

    public ChangeWorkflowService(IChangeRepository changeRepository, IApprovalRepository approvalRepository, IAuditService audit, IHttpContextAccessor contextAccessor)
    {
        _changeRepository = changeRepository;
        _approvalRepository = approvalRepository;
        _audit = audit;
        _contextAccessor = contextAccessor;
    }

    public async Task<ChangeRequest?> SubmitAsync(Guid changeId, Guid actorUserId, IReadOnlyCollection<Guid> approverUserIds, string? approvalStrategy, string? reason, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || change.StatusId != Draft) return null;

        var validationError = ValidateSubmitRequirements(change);
        if (!string.IsNullOrWhiteSpace(validationError)) throw new InvalidOperationException(validationError);

        var approvalRequired = change.ChangeTypeId != 2 || change.ApprovalRequired;
        change.ApprovalRequired = approvalRequired;
        change.ApprovalStrategy = NormalizeStrategy(approvalStrategy ?? change.ApprovalStrategy);
        change.ApprovalRequesterUserId = actorUserId;
        change.SubmittedAt = DateTime.UtcNow;
        change.SubmittedByUserId = actorUserId;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;

        if (approvalRequired)
        {
            var selectedApprovers = approverUserIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!selectedApprovers.Any())
            {
                selectedApprovers = change.ChangeApprovers.Select(x => x.ApproverUserId).Distinct().ToList();
            }

            if (!selectedApprovers.Any()) throw new InvalidOperationException("At least one approver is required when ApprovalRequired is true.");

            change.StatusId = PendingApproval;
            change.ChangeApprovers.Clear();
            change.ChangeApprovals.Clear();
            foreach (var approverId in selectedApprovers)
            {
                change.ChangeApprovers.Add(new ChangeApprover
                {
                    ChangeApproverId = Guid.NewGuid(),
                    ChangeId = change.ChangeRequestId,
                    ApproverUserId = approverId,
                    CreatedAt = DateTime.UtcNow
                });

                change.ChangeApprovals.Add(new ChangeApproval
                {
                    ChangeApprovalId = Guid.NewGuid(),
                    ChangeRequestId = change.ChangeRequestId,
                    ApproverUserId = approverId,
                    ApprovalStatusId = 1,
                    Comments = string.Empty
                });
            }
        }
        else
        {
            change.StatusId = Submitted;
        }

        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;

        await LogTransitionAsync(updated, actorUserId, "Submit", reason ?? "Submitted", new
        {
            from = "Draft",
            to = updated.Status?.Name ?? updated.StatusId.ToString(),
            approvalRequired = updated.ApprovalRequired,
            updated.ApprovalStrategy,
            approvers = updated.ChangeApprovers.Select(x => x.ApproverUserId)
        }, cancellationToken);

        return updated;
    }

    public async Task<ChangeRequest?> ApproveAsync(Guid changeId, Guid actorUserId, string? comments, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || change.StatusId != PendingApproval) return null;

        var approval = change.ChangeApprovals.FirstOrDefault(x => x.ApproverUserId == actorUserId);
        if (approval is null) return null;

        approval.ApprovalStatusId = 2;
        approval.ApprovedAt = DateTime.UtcNow;
        approval.Comments = comments ?? string.Empty;

        change.StatusId = EvaluateStatus(change);
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;

        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;

        await LogTransitionAsync(updated, actorUserId, "Approve", comments ?? "Approved", new { strategy = updated.ApprovalStrategy }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> RejectAsync(Guid changeId, Guid actorUserId, string? comments, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || change.StatusId != PendingApproval) return null;

        var approval = change.ChangeApprovals.FirstOrDefault(x => x.ApproverUserId == actorUserId);
        if (approval is null) return null;

        approval.ApprovalStatusId = 3;
        approval.ApprovedAt = DateTime.UtcNow;
        approval.Comments = comments ?? string.Empty;

        change.StatusId = Rejected;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;

        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;

        await LogTransitionAsync(updated, actorUserId, "Reject", comments ?? "Rejected", new { strategy = updated.ApprovalStrategy }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> RevertToDraftAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || (change.StatusId != Submitted && change.StatusId != PendingApproval)) return null;
        change.StatusId = Draft;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;
        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;
        await LogTransitionAsync(updated, actorUserId, "RevertToDraft", reason ?? "Reverted", new { from = change.StatusId, to = Draft }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> StartAsync(Guid changeId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || (change.StatusId != Approved && change.StatusId != Scheduled)) return null;
        if (!isAdmin && change.AssignedToUserId != actorUserId) return null;
        change.StatusId = InImplementation;
        change.ActualStart = DateTime.UtcNow;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;
        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;
        await LogTransitionAsync(updated, actorUserId, "Start", "Implementation started", new { }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> CompleteAsync(Guid changeId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || change.StatusId != InImplementation) return null;
        if (!isAdmin && change.AssignedToUserId != actorUserId) return null;
        change.StatusId = Completed;
        change.ActualEnd = DateTime.UtcNow;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;
        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;
        await LogTransitionAsync(updated, actorUserId, "Complete", "Implementation completed", new { }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> CloseAsync(Guid changeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || change.StatusId != Completed) return null;
        change.StatusId = Closed;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;
        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;
        await LogTransitionAsync(updated, actorUserId, "Close", "Closed", new { }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> CancelAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue || change.StatusId == Closed) return null;
        change.StatusId = Cancelled;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;
        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;
        await LogTransitionAsync(updated, actorUserId, "Cancel", reason ?? "Cancelled", new { }, cancellationToken);
        return updated;
    }

    public async Task<ChangeRequest?> SoftDeleteAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null || change.DeletedAt.HasValue) return null;
        change.DeletedAt = DateTime.UtcNow;
        change.DeletedByUserId = actorUserId;
        change.DeletedReason = reason;
        change.UpdatedAt = DateTime.UtcNow;
        change.UpdatedBy = actorUserId;
        var updated = await _changeRepository.UpdateAsync(change, cancellationToken);
        if (updated is null) return null;
        await LogTransitionAsync(updated, actorUserId, "Delete", reason ?? "Soft deleted", new { deleted = true }, cancellationToken);
        return updated;
    }

    private static string NormalizeStrategy(string strategy)
    {
        if (string.Equals(strategy, "All", StringComparison.OrdinalIgnoreCase)) return "All";
        if (string.Equals(strategy, "Majority", StringComparison.OrdinalIgnoreCase)) return "Majority";
        return "Any";
    }

    private static string? ValidateSubmitRequirements(ChangeRequest change)
    {
        if (string.IsNullOrWhiteSpace(change.Title)) return "Title is required before submit.";
        if (change.ChangeTypeId <= 0) return "ChangeTypeId is required before submit.";
        if (change.RiskLevelId <= 0) return "RiskLevel is required before submit.";
        if (!change.PlannedStart.HasValue) return "ImplementationDate is required before submit.";
        if (string.IsNullOrWhiteSpace(change.Description)) return "ImpactDescription is required before submit.";
        if (string.IsNullOrWhiteSpace(change.BackoutPlan)) return "RollbackPlan is required before submit.";
        return null;
    }

    private static int EvaluateStatus(ChangeRequest change)
    {
        var approvals = change.ChangeApprovals.ToList();
        var total = approvals.Count;
        var approved = approvals.Count(x => x.ApprovalStatusId == 2);
        if (approved == 0) return PendingApproval;
        var strategy = NormalizeStrategy(change.ApprovalStrategy);
        if (strategy == "All") return approved == total ? Approved : PendingApproval;
        if (strategy == "Majority") return approved > total / 2.0 ? Approved : PendingApproval;
        return Approved;
    }

    private async Task LogTransitionAsync(ChangeRequest change, Guid actorUserId, string reason, string details, object transitionDetails, CancellationToken cancellationToken)
    {
        var actorUpn = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Upn)
            ?? _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
            ?? _contextAccessor.HttpContext?.User.Identity?.Name
            ?? "unknown@local";

        await _audit.LogAsync(4, actorUserId, actorUpn, "cm", "ChangeRequest", change.ChangeRequestId, change.ChangeNumber.ToString(), reason,
            JsonSerializer.Serialize(new { details, transitionDetails }), cancellationToken);
    }
}
