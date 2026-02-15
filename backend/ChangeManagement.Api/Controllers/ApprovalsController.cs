using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes/{changeId:guid}/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService) => _approvalService = approvalService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeApproval>>> GetApprovals(Guid changeId, CancellationToken cancellationToken)
        => Ok(await _approvalService.GetApprovalsForChangeAsync(changeId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ChangeApproval>> CreateApproval(Guid changeId, [FromBody] ApprovalCreateDto request, CancellationToken cancellationToken)
    {
        var approval = new ChangeApproval
        {
            ChangeApprovalId = Guid.NewGuid(),
            ChangeRequestId = changeId,
            ApproverUserId = request.ApproverUserId,
            ApprovalStatusId = 1,
            Comments = request.Comments
        };

        var created = await _approvalService.CreateApprovalAsync(approval, cancellationToken);
        return CreatedAtAction(nameof(GetApprovals), new { changeId }, created);
    }

    [HttpPost("{approvalId:guid}/decision")]
    public async Task<ActionResult<ChangeApproval>> DecideApproval(Guid changeId, Guid approvalId, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        var approval = await _approvalService.RecordDecisionAsync(changeId, approvalId, request.ApprovalStatusId, request.Comments, cancellationToken);
        return approval is null ? NotFound() : Ok(approval);
    }
}
