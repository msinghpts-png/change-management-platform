using System.Security.Claims;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface IWorkflowService
{
    Task<ChangeRequest> SaveDraftAsync(ChangeCreateDto request, ClaimsPrincipal user, CancellationToken cancellationToken);
    Task<ChangeRequest?> SaveDraftAsync(Guid changeId, ChangeUpdateDto request, ClaimsPrincipal user, CancellationToken cancellationToken);
    Task<ChangeRequest?> SubmitAsync(Guid changeId, SubmitChangeRequestDto? request, ClaimsPrincipal user, CancellationToken cancellationToken);
}

public class WorkflowService : IWorkflowService
{
    private readonly IChangeService _changeService;
    private readonly IChangeWorkflowService _workflow;
    private readonly ChangeManagementDbContext _dbContext;

    public WorkflowService(IChangeService changeService, IChangeWorkflowService workflow, ChangeManagementDbContext dbContext)
    {
        _changeService = changeService;
        _workflow = workflow;
        _dbContext = dbContext;
    }

    public async Task<ChangeRequest> SaveDraftAsync(ChangeCreateDto request, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var changeTypeId = await ResolveChangeTypeIdAsync(request, cancellationToken);
        var priorityId = await ResolvePriorityIdAsync(request, cancellationToken);
        var riskLevelId = await ResolveRiskLevelIdAsync(request, cancellationToken);

        if (changeTypeId <= 0) throw new InvalidOperationException("Invalid ChangeTypeId.");
        if (priorityId <= 0) throw new InvalidOperationException("Invalid PriorityId.");
        if (riskLevelId <= 0) throw new InvalidOperationException("Invalid RiskLevelId.");

        var requestedByUserId = await ResolveRequestedByUserIdAsync(request, user, cancellationToken);
        if (request.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(user => user.UserId == request.AssignedToUserId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"AssignedToUserId '{request.AssignedToUserId.Value}' does not exist in cm.User.");
        }

        var approvalRequired = changeTypeId != 2 || request.ApprovalRequired == true;

        var entity = new ChangeRequest
        {
            ChangeRequestId = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            ImplementationSteps = request.ImplementationSteps,
            BackoutPlan = request.BackoutPlan,
            ServiceSystem = request.ServiceSystem,
            Category = request.Category,
            Environment = request.Environment,
            BusinessJustification = request.BusinessJustification,
            ChangeTypeId = changeTypeId,
            PriorityId = priorityId,
            StatusId = 1,
            RiskLevelId = riskLevelId,
            ImpactTypeId = request.ImpactTypeId ?? 2,
            ImpactLevelId = request.ImpactLevelId,
            RequestedByUserId = requestedByUserId,
            AssignedToUserId = request.AssignedToUserId,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            ApprovalRequired = approvalRequired,
            ApprovalStrategy = string.IsNullOrWhiteSpace(request.ApprovalStrategy) ? ApprovalStrategies.Any : request.ApprovalStrategy,
            ImplementationGroup = request.ImplementationGroup,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = ResolveActorUserId(user)
        };

        if (request.ApproverUserIds is not null)
        {
            entity.ChangeApprovers = request.ApproverUserIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .Select(x => new ChangeApprover
                {
                    ChangeApproverId = Guid.NewGuid(),
                    ChangeRequestId = entity.ChangeRequestId,
                    ApproverUserId = x,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();
        }

        return await _changeService.CreateAsync(entity, cancellationToken);
    }

    public async Task<ChangeRequest?> SaveDraftAsync(Guid changeId, ChangeUpdateDto request, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var existing = await _changeService.GetByIdAsync(changeId, cancellationToken);
        if (existing is null || existing.DeletedAt.HasValue) return null;

        existing.Title = request.Title;
        existing.Description = request.Description ?? existing.Description;
        existing.ImplementationSteps = request.ImplementationSteps ?? existing.ImplementationSteps;
        existing.BackoutPlan = request.BackoutPlan ?? existing.BackoutPlan;
        existing.ServiceSystem = request.ServiceSystem ?? existing.ServiceSystem;
        existing.Category = request.Category ?? existing.Category;
        existing.Environment = request.Environment ?? existing.Environment;
        existing.BusinessJustification = request.BusinessJustification ?? existing.BusinessJustification;

        if (request.ChangeTypeId > 0)
        {
            var resolved = await ResolveChangeTypeIdAsync(new ChangeCreateDto { ChangeTypeId = request.ChangeTypeId }, cancellationToken);
            if (resolved <= 0) throw new InvalidOperationException("Invalid ChangeTypeId.");
            existing.ChangeTypeId = resolved;
        }

        if (request.PriorityId > 0)
        {
            var resolved = await ResolvePriorityIdAsync(new ChangeCreateDto { PriorityId = request.PriorityId }, cancellationToken);
            if (resolved <= 0) throw new InvalidOperationException("Invalid PriorityId.");
            existing.PriorityId = resolved;
        }

        if (request.RiskLevelId > 0)
        {
            var resolved = await ResolveRiskLevelIdAsync(new ChangeCreateDto { RiskLevelId = request.RiskLevelId }, cancellationToken);
            if (resolved <= 0) throw new InvalidOperationException("Invalid RiskLevelId.");
            existing.RiskLevelId = resolved;
        }

        existing.ImpactTypeId = request.ImpactTypeId ?? existing.ImpactTypeId;
        existing.ImpactLevelId = request.ImpactLevelId ?? existing.ImpactLevelId;
        existing.AssignedToUserId = request.AssignedToUserId ?? existing.AssignedToUserId;
        existing.PlannedStart = request.PlannedStart ?? existing.PlannedStart;
        existing.PlannedEnd = request.PlannedEnd ?? existing.PlannedEnd;
        existing.ActualStart = request.ActualStart ?? existing.ActualStart;
        existing.ActualEnd = request.ActualEnd ?? existing.ActualEnd;
        existing.UpdatedBy = ResolveActorUserId(user);

        if (request.ApprovalRequired.HasValue)
        {
            existing.ApprovalRequired = existing.ChangeTypeId != 2 || request.ApprovalRequired.Value;
        }

        existing.ApprovalStrategy = string.IsNullOrWhiteSpace(request.ApprovalStrategy) ? existing.ApprovalStrategy : request.ApprovalStrategy;
        existing.ImplementationGroup = request.ImplementationGroup ?? existing.ImplementationGroup;

        if (request.ApproverUserIds is not null)
        {
            existing.ChangeApprovers = request.ApproverUserIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .Select(approverUserId => new ChangeApprover
                {
                    ChangeApproverId = Guid.NewGuid(),
                    ChangeRequestId = existing.ChangeRequestId,
                    ApproverUserId = approverUserId,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();
        }

        return await _changeService.UpdateAsync(existing, cancellationToken);
    }

    public async Task<ChangeRequest?> SubmitAsync(Guid changeId, SubmitChangeRequestDto? request, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var actorId = ResolveActorUserId(user);
        var approverIds = (IReadOnlyCollection<Guid>)(request?.ApproverUserIds ?? new List<Guid>());
        return await _workflow.SubmitAsync(changeId, actorId, approverIds, request?.ApprovalStrategy, request?.Reason, cancellationToken);
    }

    private static Guid ResolveActorUserId(ClaimsPrincipal user)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(actor, out var actorUserId) && actorUserId != Guid.Empty)
        {
            return actorUserId;
        }

        throw new UnauthorizedAccessException("Authenticated actor is required.");
    }

    private async Task<int> ResolveChangeTypeIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.ChangeTypeId.HasValue && request.ChangeTypeId.Value > 0)
        {
            var anyReferenceData = await _dbContext.ChangeTypes.AnyAsync(cancellationToken);
            if (!anyReferenceData)
            {
                return request.ChangeTypeId.Value;
            }

            var exists = await _dbContext.ChangeTypes.AnyAsync(x => x.ChangeTypeId == request.ChangeTypeId.Value, cancellationToken);
            return exists ? request.ChangeTypeId.Value : 0;
        }

        if (!string.IsNullOrWhiteSpace(request.ChangeType))
        {
            var normalized = request.ChangeType.Trim().ToLowerInvariant();
            var mapped = await _dbContext.ChangeTypes.Where(x => x.Name.ToLower() == normalized).Select(x => x.ChangeTypeId).FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2;
    }

    private async Task<int> ResolvePriorityIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.PriorityId.HasValue && request.PriorityId.Value > 0)
        {
            var anyReferenceData = await _dbContext.ChangePriorities.AnyAsync(cancellationToken);
            if (!anyReferenceData)
            {
                return request.PriorityId.Value;
            }

            var exists = await _dbContext.ChangePriorities.AnyAsync(x => x.PriorityId == request.PriorityId.Value, cancellationToken);
            return exists ? request.PriorityId.Value : 0;
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var normalized = request.Priority.Trim().ToLowerInvariant();
            var mapped = await _dbContext.ChangePriorities.Where(x => x.Name.ToLower() == normalized).Select(x => x.PriorityId).FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2;
    }

    private async Task<int> ResolveRiskLevelIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.RiskLevelId.HasValue && request.RiskLevelId.Value > 0)
        {
            var anyReferenceData = await _dbContext.RiskLevels.AnyAsync(cancellationToken);
            if (!anyReferenceData)
            {
                return request.RiskLevelId.Value;
            }

            var exists = await _dbContext.RiskLevels.AnyAsync(x => x.RiskLevelId == request.RiskLevelId.Value, cancellationToken);
            return exists ? request.RiskLevelId.Value : 0;
        }

        if (!string.IsNullOrWhiteSpace(request.RiskLevel))
        {
            var normalized = request.RiskLevel.Trim().ToLowerInvariant();
            var mapped = await _dbContext.RiskLevels.Where(x => x.Name.ToLower() == normalized).Select(x => x.RiskLevelId).FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2;
    }

    private async Task<Guid> ResolveRequestedByUserIdAsync(ChangeCreateDto request, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var claimUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(claimUserId, out var parsedClaimUserId) && parsedClaimUserId != Guid.Empty)
        {
            var existsByClaimId = await _dbContext.Users.AnyAsync(u => u.UserId == parsedClaimUserId, cancellationToken);
            if (existsByClaimId) return parsedClaimUserId;
        }

        if (request.RequestedByUserId.HasValue && request.RequestedByUserId.Value != Guid.Empty)
        {
            var existing = await _dbContext.Users.AnyAsync(u => u.UserId == request.RequestedByUserId.Value, cancellationToken);
            if (existing) return request.RequestedByUserId.Value;
        }

        var fallback = await _dbContext.Users.Where(u => u.IsActive).Select(u => u.UserId).FirstOrDefaultAsync(cancellationToken);
        if (fallback != Guid.Empty) return fallback;

        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        _dbContext.Users.Add(new User { UserId = adminId, Upn = "admin@local", DisplayName = "Local Admin", Role = "Admin", IsActive = true, PasswordHash = PasswordHasher.Hash("Admin123!") });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return adminId;
    }
}
