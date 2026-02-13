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
    private readonly IChangeService _changeService;

    public ApprovalsController(IApprovalService approvalService, IChangeService changeService)
    {
        _approvalService = approvalService;
        _changeService = changeService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ChangeApproval>> GetApprovals(Guid changeId)
    {
        if (_changeService.GetById(changeId) is null)
        {
            return NotFound("Change request not found.");
        }

        var approvals = _approvalService.GetApprovalsForChange(changeId);
        return Ok(approvals);
    }

    [HttpPost]
    public ActionResult<ChangeApproval> CreateApproval(Guid changeId, [FromBody] ApprovalCreateDto request)
    {
        if (_changeService.GetById(changeId) is null)
        {
            return NotFound("Change request not found.");
        }

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
        if (request.Status == ApprovalStatus.Pending)
        {
            return BadRequest("Decision status must be Approved or Rejected.");
        }

        var result = _approvalService.RecordDecision(changeId, approvalId, request.Status, request.Comment, DateTime.UtcNow);
        if (result.Approval is null)
        {
            if (string.Equals(result.Error, "Approval not found.", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(result.Error, "Change request not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Approval);
    }
}
