using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;
    private readonly IAuditService _audit;

    public ChangesController(IChangeService changeService, IAuditService audit)
    {
        _changeService = changeService;
        _audit = audit;
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
        var entity = new ChangeRequest
        {
            ChangeRequestId = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ChangeTypeId = request.ChangeTypeId,
            PriorityId = request.PriorityId,
            StatusId = 1,
            RiskLevelId = request.RiskLevelId,
            RequestedByUserId = request.RequestedByUserId,
            AssignedToUserId = request.AssignedToUserId,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.RequestedByUserId
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
}
