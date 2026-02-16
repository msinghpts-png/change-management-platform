using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<IEnumerable<ChangeRequestDto>>> GetChanges(CancellationToken cancellationToken)
    {
        var items = await _changeService.GetAllAsync(cancellationToken);
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
            approver = a.ApproverUser?.Upn ?? string.Empty,
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
            sizeBytes = 0,
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
            Description = request.Description,
            ChangeTypeId = changeTypeId,
            PriorityId = priorityId,
            StatusId = 1,
            RiskLevelId = riskLevelId,
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

        existing.Title = string.IsNullOrWhiteSpace(request.Title) ? existing.Title : request.Title;
        existing.Description = string.IsNullOrWhiteSpace(request.Description) ? existing.Description : request.Description;
        existing.ChangeTypeId = request.ChangeTypeId > 0 ? request.ChangeTypeId : existing.ChangeTypeId;
        existing.PriorityId = request.PriorityId > 0 ? request.PriorityId : existing.PriorityId;
        existing.StatusId = request.StatusId > 0 ? request.StatusId : existing.StatusId;
        existing.RiskLevelId = request.RiskLevelId > 0 ? request.RiskLevelId : existing.RiskLevelId;
        existing.AssignedToUserId = request.AssignedToUserId;
        existing.PlannedStart = request.PlannedStart;
        existing.PlannedEnd = request.PlannedEnd;
        existing.ActualStart = request.ActualStart;
        existing.ActualEnd = request.ActualEnd;
        existing.UpdatedBy = request.UpdatedBy == Guid.Empty ? existing.UpdatedBy : request.UpdatedBy;

        var updated = await _changeService.UpdateAsync(existing, cancellationToken);
        _logger.LogInformation("Updated change {ChangeRequestId}", existing.ChangeRequestId);
        return Ok(ToDto(updated!));
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

        var validationError = ValidateSubmitRequirements(existing);
        if (!string.IsNullOrEmpty(validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var validationError = ValidateSubmitRequirements(existing);
        if (!string.IsNullOrEmpty(validationError))
        {
            return BadRequest(new { message = validationError });
        }

        existing.StatusId = 2;
        existing.UpdatedBy = actorUserId == Guid.Empty ? existing.UpdatedBy : actorUserId;

        var updated = await _changeService.UpdateAsync(existing, cancellationToken);
        await _audit.LogAsync(3, actorUserId, "system@local", "cm", "ChangeRequest", existing.ChangeRequestId, existing.ChangeNumber.ToString(), "Submit", "Submitted for approval", cancellationToken);
        _logger.LogInformation("Submitted change {ChangeRequestId}", existing.ChangeRequestId);
        return Ok(ToDto(updated!));
    }


    private static string? ValidateSubmitRequirements(ChangeRequest change)
    {
        if (string.IsNullOrWhiteSpace(change.Title)) return "Title is required before submit.";
        if (change.ChangeTypeId <= 0) return "ChangeTypeId is required before submit.";
        if (change.RiskLevelId <= 0) return "RiskLevel is required before submit.";
        if (!change.PlannedStart.HasValue) return "ImplementationDate is required before submit.";

        var impactDescription = ExtractSection(change.Description, "Description");
        if (string.IsNullOrWhiteSpace(impactDescription)) return "ImpactDescription is required before submit.";

        var rollbackPlan = ExtractSection(change.Description, "Backout Plan");
        if (string.IsNullOrWhiteSpace(rollbackPlan)) return "RollbackPlan is required before submit.";

        return null;
    }

    private static string ExtractSection(string? source, string sectionName)
    {
        if (string.IsNullOrWhiteSpace(source)) return string.Empty;

        var pattern = $@"{Regex.Escape(sectionName)}:\s*(.*?)(?:\n[A-Za-z][^\n]*:\s*|$)";
        var match = Regex.Match(source, pattern, RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
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
        ChangeTypeId = change.ChangeTypeId,
        PriorityId = change.PriorityId,
        StatusId = change.StatusId,
        RiskLevelId = change.RiskLevelId,
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

        var fallbackId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fallbackExists = await _dbContext.Users.AnyAsync(user => user.UserId == fallbackId, cancellationToken);
        if (!fallbackExists)
        {
            _dbContext.Users.Add(new User { UserId = fallbackId, Upn = "system@local", DisplayName = "System User", Role = "Admin", IsActive = true, PasswordHash = string.Empty });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return fallbackId;
    }
}
