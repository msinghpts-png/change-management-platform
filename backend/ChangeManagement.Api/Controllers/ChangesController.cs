using System.Security.Claims;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;
    private readonly IChangeWorkflowService _workflow;
    private readonly ChangeManagementDbContext _dbContext;
    private readonly ILogger<ChangesController> _logger;
    private readonly IHostEnvironment _environment;

    public ChangesController(IChangeService changeService, IChangeWorkflowService workflow, ChangeManagementDbContext dbContext, ILogger<ChangesController> logger, IHostEnvironment environment)
    {
        _changeService = changeService;
        _workflow = workflow;
        _dbContext = dbContext;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeRequestDto>>> GetChanges([FromQuery] Guid? requestedByUserId, CancellationToken cancellationToken)
    {
        var items = await _changeService.GetAllAsync(cancellationToken);
        if (requestedByUserId.HasValue && requestedByUserId.Value != Guid.Empty)
        {
            items = items.Where(x => x.RequestedByUserId == requestedByUserId.Value).ToList();
        }

        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> GetChangeById(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        try
        {
            var change = await _dbContext.ChangeRequests
                .AsSplitQuery()
                .Include(c => c.ChangeType)
                .Include(c => c.Priority)
                .Include(c => c.Status)
                .Include(c => c.RiskLevel)
                .Include(c => c.ImpactLevel)
                .Include(c => c.RequestedByUser)
                .Include(c => c.AssignedToUser)
                .Include(c => c.ChangeApprovals).ThenInclude(a => a.ApprovalStatus)
                .Include(c => c.ChangeApprovals).ThenInclude(a => a.ApproverUser)
                .Include(c => c.ChangeApprovers).ThenInclude(a => a.ApproverUser)
                .Include(c => c.ChangeAttachments)
                .Include(c => c.ChangeTasks)
                .FirstOrDefaultAsync(c => c.ChangeRequestId == guidResult && c.DeletedAt == null, cancellationToken);

            return change is null ? NotFound() : Ok(ToDto(change));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error loading change {ChangeRequestId}", guidResult);
            var detail = _environment.IsDevelopment() ? ex.Message : null;
            return Problem(title: "Unable to load change request.", detail: detail, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateChange([FromBody] ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.ChangeRequestId.HasValue && request.ChangeRequestId.Value != Guid.Empty)
        {
            return BadRequest(new { message = "ChangeRequestId must not be supplied by client." });
        }

        var changeTypeId = await ResolveChangeTypeIdAsync(request, cancellationToken);
        var priorityId = await ResolvePriorityIdAsync(request, cancellationToken);
        var riskLevelId = await ResolveRiskLevelIdAsync(request, cancellationToken);

        if (changeTypeId <= 0) return BadRequest(new { message = "Invalid ChangeTypeId." });
        if (priorityId <= 0) return BadRequest(new { message = "Invalid PriorityId." });
        if (riskLevelId <= 0) return BadRequest(new { message = "Invalid RiskLevelId." });

        Guid requestedByUserId;
        try
        {
            requestedByUserId = await ResolveRequestedByUserIdAsync(request, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unable to resolve requested-by user for create change");
            return BadRequest(new { message = ex.Message });
        }
        if (request.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(user => user.UserId == request.AssignedToUserId.Value, cancellationToken))
        {
            return BadRequest($"AssignedToUserId '{request.AssignedToUserId.Value}' does not exist in cm.User.");
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
            ImpactTypeId = request.ImpactTypeId,
            ImpactLevelId = request.ImpactLevelId,
            RequestedByUserId = requestedByUserId,
            AssignedToUserId = request.AssignedToUserId,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            ApprovalRequired = approvalRequired,
            ApprovalStrategy = string.IsNullOrWhiteSpace(request.ApprovalStrategy) ? "Any" : request.ApprovalStrategy,
            ImplementationGroup = request.ImplementationGroup
        };

        if (request.ApproverUserIds?.Any() == true)
        {
            foreach (var approverUserId in request.ApproverUserIds.Where(x => x != Guid.Empty).Distinct())
            {
                entity.ChangeApprovers.Add(new ChangeApprover
                {
                    ChangeApproverId = Guid.NewGuid(),
                    ChangeRequestId = entity.ChangeRequestId,
                    ApproverUserId = approverUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var created = await _changeService.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created change {ChangeRequestId}", created.ChangeRequestId);
        return CreatedAtAction(nameof(GetChangeById), new { id = created.ChangeRequestId }, new { changeRequestId = created.ChangeRequestId });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> UpdateChange(string id, [FromBody] ChangeUpdateDto request, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var existing = await _changeService.GetByIdAsync(guidResult, cancellationToken);
        if (existing is null) return NotFound();

        existing.Title = string.IsNullOrWhiteSpace(request.Title) ? existing.Title : request.Title;
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
            if (resolved <= 0) return BadRequest(new { message = "Invalid ChangeTypeId." });
            existing.ChangeTypeId = resolved;
        }
        if (request.PriorityId > 0)
        {
            var resolved = await ResolvePriorityIdAsync(new ChangeCreateDto { PriorityId = request.PriorityId }, cancellationToken);
            if (resolved <= 0) return BadRequest(new { message = "Invalid PriorityId." });
            existing.PriorityId = resolved;
        }
        if (request.RiskLevelId > 0)
        {
            var resolved = await ResolveRiskLevelIdAsync(new ChangeCreateDto { RiskLevelId = request.RiskLevelId }, cancellationToken);
            if (resolved <= 0) return BadRequest(new { message = "Invalid RiskLevelId." });
            existing.RiskLevelId = resolved;
        }
        existing.ImpactTypeId = request.ImpactTypeId ?? existing.ImpactTypeId;
        existing.ImpactLevelId = request.ImpactLevelId ?? existing.ImpactLevelId;
        existing.AssignedToUserId = request.AssignedToUserId ?? existing.AssignedToUserId;
        existing.PlannedStart = request.PlannedStart ?? existing.PlannedStart;
        existing.PlannedEnd = request.PlannedEnd ?? existing.PlannedEnd;
        existing.ActualStart = request.ActualStart ?? existing.ActualStart;
        existing.ActualEnd = request.ActualEnd ?? existing.ActualEnd;
        existing.UpdatedBy = ResolveActorUserId();
        if (request.ApprovalRequired.HasValue)
        {
            existing.ApprovalRequired = existing.ChangeTypeId != 2 || request.ApprovalRequired.Value;
        }
        existing.ApprovalStrategy = string.IsNullOrWhiteSpace(request.ApprovalStrategy) ? existing.ApprovalStrategy : request.ApprovalStrategy;
        existing.ImplementationGroup = request.ImplementationGroup ?? existing.ImplementationGroup;

        if (request.ApproverUserIds is not null)
        {
            existing.ChangeApprovers.Clear();
            foreach (var approverUserId in request.ApproverUserIds.Where(x => x != Guid.Empty).Distinct())
            {
                existing.ChangeApprovers.Add(new ChangeApprover
                {
                    ChangeApproverId = Guid.NewGuid(),
                    ChangeRequestId = existing.ChangeRequestId,
                    ApproverUserId = approverUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var updated = await _changeService.UpdateAsync(existing, cancellationToken);
        if (updated is null)
        {
            return BadRequest(new { message = "This change cannot be edited in the current status." });
        }

        _logger.LogInformation("Updated change {ChangeRequestId}", existing.ChangeRequestId);
        return Ok(ToDto(updated));
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ChangeRequestDto>> SubmitForApproval(string id, [FromBody] SubmitChangeRequestDto? request, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var actorId = ResolveActorUserId();
            var approverIds = (IReadOnlyCollection<Guid>)(request?.ApproverUserIds ?? new List<Guid>());
            var updated = await _workflow.SubmitAsync(changeId, actorId, approverIds, request?.ApprovalStrategy, request?.Reason, cancellationToken);
            if (updated is null)
            {
                return BadRequest(new { message = "Submit action is not allowed." });
            }
            return Ok(ToDto(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ChangeRequestDto>> Approve(string id, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.ApproveAsync(changeId, ResolveActorUserId(), request.Comments, cancellationToken);
            return updated is null ? BadRequest(new { message = "Approval action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ChangeRequestDto>> Reject(string id, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.RejectAsync(changeId, ResolveActorUserId(), request.Comments, cancellationToken);
            return updated is null ? BadRequest(new { message = "Rejection action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("{id}/revert-to-draft")]
    public async Task<ActionResult<ChangeRequestDto>> RevertToDraft(string id, [FromBody] WorkflowActionRequestDto? request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.RevertToDraftAsync(changeId, ResolveActorUserId(), request?.Reason, cancellationToken);
            return updated is null ? BadRequest(new { message = "Revert action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<ChangeRequestDto>> Start(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.StartAsync(changeId, ResolveActorUserId(), User.IsInRole("Admin"), cancellationToken);
            return updated is null ? BadRequest(new { message = "Start action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ChangeRequestDto>> Complete(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.CompleteAsync(changeId, ResolveActorUserId(), User.IsInRole("Admin"), cancellationToken);
            return updated is null ? BadRequest(new { message = "Complete action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<ChangeRequestDto>> Close(string id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.CloseAsync(changeId, ResolveActorUserId(), cancellationToken);
            return updated is null ? BadRequest(new { message = "Close action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ChangeRequestDto>> Cancel(string id, [FromBody] WorkflowActionRequestDto? request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.CancelAsync(changeId, ResolveActorUserId(), request?.Reason, cancellationToken);
            return updated is null ? BadRequest(new { message = "Cancel action is not allowed." }) : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> Delete(string id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflow.SoftDeleteAsync(changeId, ResolveActorUserId(), reason, cancellationToken);
            return updated is null ? NotFound() : Ok(ToDto(updated));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    private bool TryParseId(string id, out Guid guidResult, out BadRequestObjectResult badRequest)
    {
        if (!Guid.TryParse(id, out guidResult))
        {
            _logger.LogWarning("Invalid change id received: {Id}", id);
            badRequest = BadRequest(new { message = "Invalid change request id." });
            return false;
        }

        badRequest = null!;
        return true;
    }

    private Guid ResolveActorUserId()
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(actor, out var actorUserId) && actorUserId != Guid.Empty)
        {
            return actorUserId;
        }

        throw new UnauthorizedAccessException("Authenticated actor is required.");
    }

    private static ChangeRequestDto ToDto(ChangeRequest change) => new()
    {
        Id = change.ChangeRequestId,
        ChangeRequestId = change.ChangeRequestId,
        ChangeNumber = change.ChangeNumber,
        Title = change.Title,
        Description = change.Description,
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
        ImpactLevel = change.ImpactLevel?.Name,
        RequestedBy = change.RequestedByUser?.Upn,
        RequestedByUserId = change.RequestedByUserId == Guid.Empty ? null : change.RequestedByUserId,
        AssignedToUserId = change.AssignedToUserId,
        Owner = change.RequestedByUser?.DisplayName ?? change.RequestedByUser?.Upn,
        RequestedByDisplay = change.RequestedByUser?.DisplayName ?? change.RequestedByUser?.Upn,
        Executor = change.AssignedToUser?.DisplayName ?? change.AssignedToUser?.Upn,
        ImplementationGroup = change.ImplementationGroup,
        ApprovalRequired = change.ApprovalRequired,
        ApprovalStrategy = change.ApprovalStrategy,
        ApproverUserIds = change.ChangeApprovers?.Select(x => x.ApproverUserId).ToList() ?? new List<Guid>(),
        Approvals = change.ChangeApprovals?.Select(a => new ApprovalDecisionItemDto
        {
            ApproverUserId = a.ApproverUserId,
            Approver = a.ApproverUser?.DisplayName ?? a.ApproverUser?.Upn ?? string.Empty,
            Status = a.ApprovalStatus?.Name ?? "Pending",
            Comments = a.Comments,
            DecisionAt = a.ApprovedAt
        }).ToList() ?? new List<ApprovalDecisionItemDto>(),
        PlannedStart = change.PlannedStart,
        PlannedEnd = change.PlannedEnd,
        CreatedAt = change.CreatedAt,
        UpdatedAt = change.UpdatedAt
    };

    private async Task<int> ResolveChangeTypeIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.ChangeTypeId.HasValue && request.ChangeTypeId.Value > 0)
        {
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

    private async Task<Guid> ResolveRequestedByUserIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(claimUserId, out var parsedClaimUserId) && parsedClaimUserId != Guid.Empty)
        {
            var existsByClaimId = await _dbContext.Users.AnyAsync(user => user.UserId == parsedClaimUserId, cancellationToken);
            if (existsByClaimId) return parsedClaimUserId;
        }

        if (request.RequestedByUserId.HasValue && request.RequestedByUserId.Value != Guid.Empty)
        {
            var existing = await _dbContext.Users.AnyAsync(user => user.UserId == request.RequestedByUserId.Value, cancellationToken);
            if (existing) return request.RequestedByUserId.Value;
        }

        var fallback = await _dbContext.Users.Where(user => user.IsActive).Select(user => user.UserId).FirstOrDefaultAsync(cancellationToken);
        if (fallback != Guid.Empty) return fallback;

        _logger.LogWarning("Unable to resolve requested-by user id: no active users in database.");
        throw new InvalidOperationException("RequestedByUserId could not be resolved.");
    }
}
