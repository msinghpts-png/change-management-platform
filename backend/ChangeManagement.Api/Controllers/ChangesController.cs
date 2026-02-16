using System.Security.Claims;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;
    private readonly IApprovalService _approvalService;
    private readonly IChangeTaskService _taskService;
    private readonly IAttachmentService _attachmentService;

    public ChangesController(IChangeService changeService, IApprovalService approvalService, IChangeTaskService taskService, IAttachmentService attachmentService)
    {
        _changeService = changeService;
        _approvalService = approvalService;
        _taskService = taskService;
        _attachmentService = attachmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeRequestDto>>> GetAll(CancellationToken cancellationToken)
        => Ok((await _changeService.GetAllAsync(cancellationToken)).Select(ToDto));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChangeRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var change = await _changeService.GetByIdAsync(id, cancellationToken);
        return change is null ? NotFound() : Ok(ToDto(change));
    }

    [HttpPost]
    public async Task<ActionResult<ChangeRequestDto>> Create([FromBody] ChangeCreateDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _changeService.CreateAsync(request, userId.Value, cancellationToken);
        if (result.Change is null) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Change.ChangeId }, ToDto(result.Change));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChangeRequestDto>> Update(Guid id, [FromBody] ChangeUpdateDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _changeService.UpdateAsync(id, request, userId.Value, cancellationToken);
        if (result.Change is null) return BadRequest(result.Error);

        return Ok(ToDto(result.Change));
    }

    [HttpPost("{id:guid}/submit")]
    public Task<ActionResult<ChangeRequestDto>> Submit(Guid id, CancellationToken cancellationToken) => Transition(id, ChangeStatus.Submitted, cancellationToken);

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "CAB")]
    public async Task<ActionResult<ChangeRequestDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await _approvalService.CreateDecisionAsync(id, true, "Approved by CAB", userId.Value, cancellationToken);
        return await Transition(id, ChangeStatus.Approved, cancellationToken);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "CAB")]
    public async Task<ActionResult<ChangeRequestDto>> Reject(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await _approvalService.CreateDecisionAsync(id, false, "Rejected by CAB", userId.Value, cancellationToken);
        return await Transition(id, ChangeStatus.Rejected, cancellationToken);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ChangeRequestDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        var change = await _changeService.GetByIdAsync(id, cancellationToken);
        var userId = GetUserId();
        if (change is null) return NotFound();
        if (userId is null) return Unauthorized();
        if (change.AssignedToUserId != userId) return Forbid();

        return await Transition(id, ChangeStatus.InImplementation, cancellationToken);
    }

    [HttpPost("{id:guid}/complete")]
    public Task<ActionResult<ChangeRequestDto>> Complete(Guid id, CancellationToken cancellationToken) => Transition(id, ChangeStatus.Completed, cancellationToken);

    [HttpPost("{id:guid}/tasks")]
    public async Task<ActionResult<object>> AddTask(Guid id, [FromBody] ChangeTaskCreateDto request, CancellationToken cancellationToken)
    {
        var task = await _taskService.CreateAsync(new ChangeTask
        {
            ChangeTaskId = Guid.NewGuid(),
            ChangeId = id,
            Title = request.Title,
            Description = request.Description,
            AssignedToUserId = request.AssignedToUserId,
            DueDate = request.DueDate
        }, cancellationToken);

        return Ok(new { task.ChangeTaskId, task.ChangeId, task.Title, task.Description, task.AssignedToUserId, task.DueDate, task.CompletedDate });
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<ActionResult<object>> AddAttachment(Guid id, [FromForm] AttachmentUploadDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _attachmentService.UploadAsync(id, request.File, userId.Value, cancellationToken);
        if (result.Attachment is null) return BadRequest(result.Error);

        return Ok(new
        {
            result.Attachment.ChangeAttachmentId,
            result.Attachment.ChangeId,
            result.Attachment.FileName,
            result.Attachment.ContentType,
            result.Attachment.FileSizeBytes,
            result.Attachment.UploadedAt
        });
    }

    private async Task<ActionResult<ChangeRequestDto>> Transition(Guid id, ChangeStatus status, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _changeService.TransitionAsync(id, status, userId.Value, cancellationToken);
        if (result.Change is null) return BadRequest(result.Error);
        return Ok(ToDto(result.Change));
    }

    private Guid? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }

    private static ChangeRequestDto ToDto(ChangeRequest change) => new()
    {
        ChangeId = change.ChangeId,
        ChangeNumber = change.ChangeNumber,
        Title = change.Title,
        Description = change.Description,
        ChangeType = change.ChangeType,
        RiskLevel = change.RiskLevel,
        Status = change.Status,
        ImpactDescription = change.ImpactDescription,
        RollbackPlan = change.RollbackPlan,
        ImplementationDate = change.ImplementationDate,
        ImplementationStartDate = change.ImplementationStartDate,
        CompletedDate = change.CompletedDate,
        CreatedByUserId = change.CreatedByUserId,
        AssignedToUserId = change.AssignedToUserId,
        ApprovedDate = change.ApprovedDate,
        CreatedAt = change.CreatedAt,
        UpdatedAt = change.UpdatedAt
    };
}
