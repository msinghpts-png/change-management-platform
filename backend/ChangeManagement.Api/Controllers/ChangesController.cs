using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IApprovalService _approvalService;
    private readonly IChangeService _changeService;
    private readonly IChangeStatusValidator _statusValidator;

    public ChangesController(IChangeService changeService, IApprovalService approvalService, IChangeStatusValidator statusValidator)
    {
        _changeService = changeService;
        _approvalService = approvalService;
        _statusValidator = statusValidator;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ChangeRequestDto>> GetChanges()
    {
        var changes = _changeService.GetAll();

        var results = changes.Select(change =>
        {
            var approvals = _approvalService.GetApprovalsForChange(change.Id).ToList();
            return MapToDto(change, approvals);
        });

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ChangeRequestDto> GetChangeById(Guid id)
    {
        var change = _changeService.GetById(id);
        if (change is null)
        {
            return NotFound();
        }

        var approvals = _approvalService.GetApprovalsForChange(change.Id).ToList();
        return Ok(MapToDto(change, approvals));
    }

    [HttpPost]
    public ActionResult<ChangeRequestDto> CreateChange([FromBody] ChangeCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var created = new ChangeRequest
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = ChangeStatus.Draft,
            Priority = request.Priority,
            RiskLevel = request.RiskLevel,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            CreatedAt = DateTime.UtcNow
        };

        var stored = _changeService.Create(created);

        return CreatedAtAction(nameof(GetChangeById), new { id = stored.Id }, MapToDto(stored, new List<ChangeApproval>()));
    }

    [HttpPut("{id:guid}")]
    public ActionResult<ChangeRequestDto> UpdateChange(Guid id, [FromBody] ChangeUpdateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var updated = new ChangeRequest
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Status = Enum.TryParse<ChangeStatus>(request.Status, true, out var status)
                ? status
                : ChangeStatus.Draft,
            Priority = request.Priority,
            RiskLevel = request.RiskLevel,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        var stored = _changeService.Update(updated);
        if (stored is null)
        {
            return NotFound();
        }

        var approvals = _approvalService.GetApprovalsForChange(stored.Id).ToList();
        return Ok(MapToDto(stored, approvals));
    }

    [HttpPost("{id:guid}/submit")]
    public ActionResult<ChangeRequestDto> SubmitForApproval(Guid id)
    {
        var change = _changeService.GetById(id);
        if (change is null)
        {
            return NotFound();
        }

        if (!_statusValidator.CanTransition(change.Status, ChangeStatus.PendingApproval))
        {
            return BadRequest("Invalid status transition.");
        }

        change.Status = ChangeStatus.PendingApproval;
        change.UpdatedAt = DateTime.UtcNow;
        var updated = _changeService.Update(change);

        if (updated is null)
        {
            return NotFound();
        }

        var approvals = _approvalService.GetApprovalsForChange(updated.Id).ToList();
        return Ok(MapToDto(updated, approvals));
    }

    private static ChangeRequestDto MapToDto(ChangeRequest change, IReadOnlyCollection<ChangeApproval> approvals)
    {
        var approvedCount = approvals.Count(approval => approval.Status == ApprovalStatus.Approved);
        var rejectedCount = approvals.Count(approval => approval.Status == ApprovalStatus.Rejected);
        var pendingCount = approvals.Count(approval => approval.Status == ApprovalStatus.Pending);

        return new ChangeRequestDto
        {
            Id = change.Id,
            Title = change.Title,
            Description = change.Description,
            Status = change.Status.ToString(),
            Priority = change.Priority,
            RiskLevel = change.RiskLevel,
            PlannedStart = change.PlannedStart,
            PlannedEnd = change.PlannedEnd,
            CreatedAt = change.CreatedAt,
            UpdatedAt = change.UpdatedAt,
            ApprovalsTotal = approvals.Count,
            ApprovalsApproved = approvedCount,
            ApprovalsRejected = rejectedCount,
            ApprovalsPending = pendingCount
        };
    }
}
