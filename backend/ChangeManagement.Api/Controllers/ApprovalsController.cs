using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes/{changeId:guid}/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ChangeApproval>> GetApprovals(Guid changeId)
    {
        var approvals = _approvalService.GetApprovalsForChange(changeId);
        return Ok(approvals);
    }

    [HttpPost]
    public ActionResult<ChangeApproval> CreateApproval(Guid changeId, [FromBody] ApprovalCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Approver))
        {
            return BadRequest("Approver is required.");
        }

        var approval = new ChangeApproval
        {
            Id = Guid.NewGuid(),
            ChangeRequestId = changeId,
            Approver = request.Approver,
            Status = ApprovalStatus.Pending,
            Comment = request.Comment
        };

        var created = _approvalService.CreateApproval(approval);
        return CreatedAtAction(nameof(GetApprovals), new { changeId }, created);
    }

    [HttpPost("{approvalId:guid}/decision")]
    public ActionResult<ChangeApproval> DecideApproval(Guid changeId, Guid approvalId, [FromBody] ApprovalDecisionDto request)
    {
        var updated = _approvalService.RecordDecision(approvalId, request.Status, request.Comment, DateTime.UtcNow);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }
}
