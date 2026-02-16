using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;
    private readonly IAuditService _audit;
    private readonly ChangeManagementDbContext _dbContext;

    public ChangesController(IChangeService changeService, IAuditService audit, ChangeManagementDbContext dbContext)
    {
        _changeService = changeService;
        _audit = audit;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeRequestDto>>> GetChanges(CancellationToken cancellationToken)
    {
        var items = await _changeService.GetAllAsync(cancellationToken);
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChangeRequestDto>> GetChangeById(Guid id, CancellationToken cancellationToken)
    {
        var change = await _changeService.GetByIdAsync(id, cancellationToken);
        return change is null ? NotFound() : Ok(ToDto(change));
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
            PlannedEnd = request.PlannedEnd,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = requestedByUserId
        };

        var created = await _changeService.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetChangeById), new { id = created.ChangeRequestId }, ToDto(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChangeRequestDto>> UpdateChange(Guid id, [FromBody] ChangeUpdateDto request, CancellationToken cancellationToken)
    {
        var existing = await _changeService.GetByIdAsync(id, cancellationToken);
        if (existing is null) return NotFound();

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.ChangeTypeId = request.ChangeTypeId;
        existing.PriorityId = request.PriorityId;
        existing.StatusId = request.StatusId;
        existing.RiskLevelId = request.RiskLevelId;
        existing.AssignedToUserId = request.AssignedToUserId;
        existing.PlannedStart = request.PlannedStart;
        existing.PlannedEnd = request.PlannedEnd;
        existing.ActualStart = request.ActualStart;
        existing.ActualEnd = request.ActualEnd;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = request.UpdatedBy;

        var updated = await _changeService.UpdateAsync(existing, cancellationToken);
        return Ok(ToDto(updated!));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<ChangeRequestDto>> SubmitForApproval(Guid id, [FromQuery] Guid actorUserId, CancellationToken cancellationToken)
    {
        if (actorUserId != Guid.Empty &&
            !await _dbContext.Users.AnyAsync(user => user.UserId == actorUserId, cancellationToken))
        {
            return BadRequest($"actorUserId '{actorUserId}' does not exist in cm.User.");
        }

        var existing = await _changeService.GetByIdAsync(id, cancellationToken);
        if (existing is null) return NotFound();

        existing.StatusId = 2;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = actorUserId;

        var updated = await _changeService.UpdateAsync(existing, cancellationToken);
        await _audit.LogAsync(3, actorUserId, "system@local", "cm", "ChangeRequest", existing.ChangeRequestId, existing.ChangeNumber.ToString(), "Submit", "Submitted for approval", cancellationToken);
        return Ok(ToDto(updated!));
    }

    private static ChangeRequestDto ToDto(ChangeRequest change) => new()
    {
        ChangeRequestId = change.ChangeRequestId,
        ChangeNumber = change.ChangeNumber,
        Title = change.Title,
        Description = change.Description,
        ChangeTypeId = change.ChangeTypeId,
        PriorityId = change.PriorityId,
        StatusId = change.StatusId,
        RiskLevelId = change.RiskLevelId
    };

    private async Task<int> ResolveChangeTypeIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.ChangeTypeId.HasValue && request.ChangeTypeId.Value > 0)
        {
            return request.ChangeTypeId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.ChangeType))
        {
            var normalized = request.ChangeType.Trim().ToLowerInvariant();
            var mapped = await _dbContext.ChangeTypes
                .Where(x => x.Name.ToLower() == normalized)
                .Select(x => x.ChangeTypeId)
                .FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2; // Normal
    }

    private async Task<int> ResolvePriorityIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.PriorityId.HasValue && request.PriorityId.Value > 0)
        {
            return request.PriorityId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var normalized = request.Priority.Trim().ToLowerInvariant();
            var mapped = await _dbContext.ChangePriorities
                .Where(x => x.Name.ToLower() == normalized || (normalized == "p1" && x.Name == "Critical") || (normalized == "p2" && x.Name == "High") || (normalized == "p3" && x.Name == "Medium") || (normalized == "p4" && x.Name == "Low"))
                .Select(x => x.PriorityId)
                .FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2; // Medium
    }

    private async Task<int> ResolveRiskLevelIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.RiskLevelId.HasValue && request.RiskLevelId.Value > 0)
        {
            return request.RiskLevelId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.RiskLevel))
        {
            var normalized = request.RiskLevel.Trim().ToLowerInvariant();
            var mapped = await _dbContext.RiskLevels
                .Where(x => x.Name.ToLower() == normalized)
                .Select(x => x.RiskLevelId)
                .FirstOrDefaultAsync(cancellationToken);
            if (mapped > 0) return mapped;
        }

        return 2; // Medium
    }

    private async Task<Guid> ResolveRequestedByUserIdAsync(ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.RequestedByUserId.HasValue && request.RequestedByUserId.Value != Guid.Empty)
        {
            var existing = await _dbContext.Users.AnyAsync(user => user.UserId == request.RequestedByUserId.Value, cancellationToken);
            if (existing)
            {
                return request.RequestedByUserId.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.RequestedBy))
        {
            var existingByUpn = await _dbContext.Users
                .Where(user => user.Upn == request.RequestedBy)
                .Select(user => user.UserId)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingByUpn != Guid.Empty)
            {
                return existingByUpn;
            }
        }

        var fallbackId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fallbackExists = await _dbContext.Users.AnyAsync(user => user.UserId == fallbackId, cancellationToken);
        if (!fallbackExists)
        {
            _dbContext.Users.Add(new User
            {
                UserId = fallbackId,
                Upn = string.IsNullOrWhiteSpace(request.RequestedBy) ? "system@local" : request.RequestedBy!,
                DisplayName = "System User",
                Role = "Admin",
                IsActive = true
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return fallbackId;
    }
}
