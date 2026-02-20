using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;

namespace ChangeManagement.Api.Extensions;

public static class ChangeRequestMappings
{
    public static ApprovalDecisionItemDto ToDto(this ChangeApprover approver)
    {
        return new ApprovalDecisionItemDto
        {
            Id = approver.ChangeApproverId,
            ApproverUserId = approver.ApproverUserId,
            Approver = approver.ApproverUser?.DisplayName ?? approver.ApproverUser?.Upn ?? string.Empty,
            Status = string.IsNullOrWhiteSpace(approver.ApprovalStatus) ? "Pending" : approver.ApprovalStatus,
            Comments = approver.DecisionComments,
            DecisionAt = approver.DecisionAt
        };
    }

    public static ChangeRequestDto ToDto(this ChangeRequest change)
    {
        var approvals = change.ChangeApprovers?.Select(ToDto).ToList() ?? new List<ApprovalDecisionItemDto>();

        return new ChangeRequestDto
        {
            Id = change.ChangeRequestId,
            ChangeRequestId = change.ChangeRequestId,
            ChangeNumber = change.ChangeNumber,
            Title = change.Title,
            Description = change.Description ?? string.Empty,
            ImplementationSteps = change.ImplementationSteps,
            BackoutPlan = change.BackoutPlan,
            ServiceSystem = change.ServiceSystem,
            Category = change.Category,
            Environment = change.Environment,
            BusinessJustification = change.BusinessJustification,
            ChangeTypeId = change.ChangeTypeId,
            PriorityId = change.PriorityId,
            StatusId = change.StatusId,
            RiskLevelId = change.RiskLevelId,
            ImpactTypeId = change.ImpactTypeId,
            Status = change.Status?.Name,
            Priority = change.Priority?.Name,
            RiskLevel = change.RiskLevel?.Name,
            RequestedBy = change.RequestedByUser?.Upn,
            RequestedByUserId = change.RequestedByUserId == Guid.Empty ? null : change.RequestedByUserId,
            AssignedToUserId = change.AssignedToUserId,
            Owner = change.RequestedByUser?.DisplayName ?? change.RequestedByUser?.Upn,
            RequestedByDisplay = change.RequestedByUser?.DisplayName ?? change.RequestedByUser?.Upn,
            Executor = change.AssignedToUser?.DisplayName ?? change.AssignedToUser?.Upn,
            ImplementationGroup = change.ImplementationGroup,
            ApprovalRequired = change.ApprovalRequired,
            ApprovalStrategy = string.IsNullOrWhiteSpace(change.ApprovalStrategy) ? "Single" : change.ApprovalStrategy,
            ApproverUserIds = change.ChangeApprovers?.Select(x => x.ApproverUserId).ToList() ?? new List<Guid>(),
            Approvals = approvals,
            ApprovalsTotal = approvals.Count,
            ApprovalsApproved = approvals.Count(x => string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase)),
            ApprovalsRejected = approvals.Count(x => string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)),
            ApprovalsPending = approvals.Count(x => string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase)),
            Attachments = change.ChangeAttachments?.Select(AttachmentMappings.ToDto).ToList() ?? new List<AttachmentDto>(),
            PlannedStart = change.PlannedStart,
            PlannedEnd = change.PlannedEnd,
            CreatedAt = change.CreatedAt,
            UpdatedAt = change.UpdatedAt
        };
    }
}
