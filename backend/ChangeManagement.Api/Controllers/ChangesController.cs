using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using ChangeManagement.Api.Security;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;
    private readonly IApprovalService _approvalService;
    private readonly IAttachmentService _attachmentService;
    private readonly IChangeTaskService _taskService;
    private readonly IAuditService _audit;
    private readonly ChangeManagementDbContext _dbContext;
    private readonly ILogger<ChangesController> _logger;

    public ChangesController(
        IChangeService changeService,
        IApprovalService approvalService,
        IAttachmentService attachmentService,
        IChangeTaskService taskService,
        IAuditService audit,
        ChangeManagementDbContext dbContext,
        ILogger<ChangesController> logger)
    {
        _changeService = changeService;
        _approvalService = approvalService;
        _attachmentService = attachmentService;
        _taskService = taskService;
        _audit = audit;
        _dbContext = dbContext;
        _logger = logger;
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

        var change = await _changeService.GetByIdAsync(guidResult, cancellationToken);
        return change is null ? NotFound() : Ok(ToDto(change));
    }

    [HttpGet("{id}/approvals")]
    public async Task<ActionResult<IEnumerable<object>>> GetApprovals(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var approvals = await _approvalService.GetApprovalsForChangeAsync(guidResult, cancellationToken);
        return Ok(approvals.Select(a => new
        {
            id = a.ChangeApprovalId,
            changeRequestId = a.ChangeRequestId,
            approver = a.ApproverUser?.DisplayName ?? a.ApproverUser?.Upn ?? string.Empty,
            status = a.ApprovalStatus?.Name ?? "Pending",
            comment = a.Comments,
            decisionAt = a.ApprovedAt
        }));
    }

    [HttpGet("{id}/attachments")]
    public async Task<ActionResult<IEnumerable<object>>> GetAttachments(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var attachments = await _attachmentService.GetForChangeAsync(guidResult, cancellationToken);
        return Ok(attachments.Select(a => new
        {
            id = a.ChangeAttachmentId,
            changeRequestId = a.ChangeRequestId,
            fileName = a.FileName,
            contentType = "application/octet-stream",
            sizeBytes = a.FileSizeBytes,
            uploadedAt = a.UploadedAt
        }));
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<object>>> GetTasks(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var tasks = await _taskService.GetByChangeAsync(guidResult, cancellationToken);
        return Ok(tasks.Select(t => new
        {
            id = t.ChangeTaskId,
            changeRequestId = t.ChangeRequestId,
            title = t.Title,
            description = t.Description,
            status = t.Status?.Name,
            dueAt = t.DueAt,
            completedAt = t.CompletedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ChangeRequestDto>> CreateChange([FromBody] ChangeCreateDto request, CancellationToken cancellationToken)
    {
        var changeTypeId = await ResolveChangeTypeIdAsync(request, cancellationToken);
        var priorityId = await ResolvePriorityIdAsync(request, cancellationToken);
        var riskLevelId = await ResolveRiskLevelIdAsync(request, cancellationToken);

        var requestedByUserId = await ResolveRequestedByUserIdAsync(request, cancellationToken);
        if (request.AssignedToUserId.HasValue &&
            !await _dbContext.Users.AnyAsync(user => user.UserId == request.AssignedToUserId.Value, cancellationToken))
        {
            return BadRequest($"AssignedToUserId '{request.AssignedToUserId.Value}' does not exist in cm.User.");
        }

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
            RequestedByUserId = requestedByUserId,
            AssignedToUserId = request.AssignedToUserId,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd
        };

        var created = await _changeService.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created change {ChangeRequestId}", created.ChangeRequestId);
        return CreatedAtAction(nameof(GetChangeById), new { id = created.ChangeRequestId }, ToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> UpdateChange(string id, [FromBody] ChangeUpdateDto request, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var existing = await _changeService.GetByIdAsync(guidResult, cancellationToken);
        if (existing is null) return NotFound();

        if (existing.StatusId == 2)
        {
            existing.BusinessJustification = request.BusinessJustification ?? existing.BusinessJustification;
            existing.AssignedToUserId = request.AssignedToUserId;
            existing.PlannedStart = request.PlannedStart;
            existing.PlannedEnd = request.PlannedEnd;
            existing.ImpactTypeId = request.ImpactTypeId ?? existing.ImpactTypeId;
            existing.UpdatedBy = request.UpdatedBy == Guid.Empty ? existing.UpdatedBy : request.UpdatedBy;
        }
        else
        {
            existing.Title = string.IsNullOrWhiteSpace(request.Title) ? existing.Title : request.Title;
            existing.Description = request.Description ?? existing.Description;
            existing.ImplementationSteps = request.ImplementationSteps ?? existing.ImplementationSteps;
            existing.BackoutPlan = request.BackoutPlan ?? existing.BackoutPlan;
            existing.ServiceSystem = request.ServiceSystem ?? existing.ServiceSystem;
            existing.Category = request.Category ?? existing.Category;
            existing.Environment = request.Environment ?? existing.Environment;
            existing.BusinessJustification = request.BusinessJustification ?? existing.BusinessJustification;
            existing.ChangeTypeId = request.ChangeTypeId > 0 ? request.ChangeTypeId : existing.ChangeTypeId;
            existing.PriorityId = request.PriorityId > 0 ? request.PriorityId : existing.PriorityId;
            existing.StatusId = request.StatusId > 0 ? request.StatusId : existing.StatusId;
            existing.RiskLevelId = request.RiskLevelId > 0 ? request.RiskLevelId : existing.RiskLevelId;
            existing.ImpactTypeId = request.ImpactTypeId ?? existing.ImpactTypeId;
            existing.AssignedToUserId = request.AssignedToUserId;
            existing.PlannedStart = request.PlannedStart;
            existing.PlannedEnd = request.PlannedEnd;
            existing.ActualStart = request.ActualStart;
            existing.ActualEnd = request.ActualEnd;
            existing.UpdatedBy = request.UpdatedBy == Guid.Empty ? existing.UpdatedBy : request.UpdatedBy;
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
    public async Task<ActionResult<ChangeRequestDto>> SubmitForApproval(string id, [FromQuery] Guid actorUserId, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        if (actorUserId != Guid.Empty &&
            !await _dbContext.Users.AnyAsync(user => user.UserId == actorUserId, cancellationToken))
        {
            return BadRequest($"actorUserId '{actorUserId}' does not exist in cm.User.");
        }

        var existing = await _changeService.GetByIdAsync(guidResult, cancellationToken);
        if (existing is null) return NotFound();
        if (existing.StatusId != 1)
        {
            return BadRequest(new { message = "Only Draft changes can be submitted for approval." });
        }

        var submitValidationError = ValidateSubmitRequirements(existing);
        if (!string.IsNullOrEmpty(submitValidationError))
        {
            return BadRequest(new { message = submitValidationError });
        }

        existing.StatusId = 2;
        existing.UpdatedBy = actorUserId == Guid.Empty ? existing.UpdatedBy : actorUserId;

        var updated = await _changeService.UpdateAsync(existing, cancellationToken);
        var actorUpn = User.FindFirstValue(ClaimTypes.Upn) ?? User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "unknown@local";
        await _audit.LogAsync(3, actorUserId, actorUpn, "cm", "ChangeRequest", existing.ChangeRequestId, existing.ChangeNumber.ToString(), "Submit", "Submitted for approval", cancellationToken);
        _logger.LogInformation("Submitted change {ChangeRequestId}", existing.ChangeRequestId);
        return Ok(ToDto(updated!));
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


    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ChangeRequestDto>> Approve(string id, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(actor, out var actorUserId) || actorUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var approval = await _approvalService.ApproveChangeAsync(guidResult, actorUserId, request.Comments ?? string.Empty, cancellationToken);
        if (approval is null) return NotFound();

        var change = await _changeService.GetByIdAsync(guidResult, cancellationToken);
        return change is null ? NotFound() : Ok(ToDto(change));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ChangeRequestDto>> Reject(string id, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var guidResult, out var badRequest)) return badRequest;

        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(actor, out var actorUserId) || actorUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var approval = await _approvalService.RejectChangeAsync(guidResult, actorUserId, request.Comments ?? string.Empty, cancellationToken);
        if (approval is null) return NotFound();

        var change = await _changeService.GetByIdAsync(guidResult, cancellationToken);
        return change is null ? NotFound() : Ok(ToDto(change));
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
        RequestedBy = change.RequestedByUser?.Upn,
        PlannedStart = change.PlannedStart,
        PlannedEnd = change.PlannedEnd,
        CreatedAt = change.CreatedAt,
        UpdatedAt = change.UpdatedAt
    };

    private async Task<int> ResolveChangeTypeIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.ChangeTypeId.HasValue && request.ChangeTypeId.Value > 0) return request.ChangeTypeId.Value;
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
        if (request.PriorityId.HasValue && request.PriorityId.Value > 0) return request.PriorityId.Value;
        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var normalized = request.Priority.Trim().ToLowerInvariant();
            var mapped = await _dbContext.ChangePriorities
                .Where(x => x.Name.ToLower() == normalized || (normalized == "p1" && x.Name == "Critical") || (normalized == "p2" && x.Name == "High") || (normalized == "p3" && x.Name == "Medium") || (normalized == "p4" && x.Name == "Low"))
                .Select(x => x.PriorityId)
                .FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2;
    }

    private async Task<int> ResolveRiskLevelIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.RiskLevelId.HasValue && request.RiskLevelId.Value > 0) return request.RiskLevelId.Value;
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

        if (!string.IsNullOrWhiteSpace(request.RequestedBy))
        {
            var existingByUpn = await _dbContext.Users.Where(user => user.Upn == request.RequestedBy).Select(user => user.UserId).FirstOrDefaultAsync(cancellationToken);
            if (existingByUpn != Guid.Empty) return existingByUpn;
        }

        var claimUpn = User.FindFirstValue(ClaimTypes.Upn) ?? User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(claimUpn))
        {
            var existingByClaimUpn = await _dbContext.Users.Where(user => user.Upn == claimUpn).Select(user => user.UserId).FirstOrDefaultAsync(cancellationToken);
            if (existingByClaimUpn != Guid.Empty) return existingByClaimUpn;
        }

                var fallback = await _dbContext.Users.Where(user => user.IsActive).Select(user => user.UserId).FirstOrDefaultAsync(cancellationToken);
        if (fallback != Guid.Empty) return fallback;

        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        _dbContext.Users.Add(new User { UserId = adminId, Upn = "admin@local", DisplayName = "Local Admin", Role = "Admin", IsActive = true, PasswordHash = PasswordHasher.Hash("Admin123!") });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return adminId;
    }
}
