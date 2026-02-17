using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes/{changeId:guid}/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;
    private readonly ChangeManagementDbContext _dbContext;

    public ApprovalsController(IApprovalService approvalService, ChangeManagementDbContext dbContext)
    {
        _approvalService = approvalService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetApprovals(Guid changeId, CancellationToken cancellationToken)
    {
        var approvals = await _approvalService.GetApprovalsForChangeAsync(changeId, cancellationToken);
        return Ok(approvals.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateApproval(Guid changeId, [FromBody] ApprovalCreateDto request, CancellationToken cancellationToken)
    {
        var changeExists = await _dbContext.ChangeRequests.AnyAsync(x => x.ChangeRequestId == changeId, cancellationToken);
        if (!changeExists)
        {
            return NotFound(new { message = "Change request not found." });
        }

        var approverUserId = request.ApproverUserId ?? Guid.Empty;
        if (approverUserId == Guid.Empty && !string.IsNullOrWhiteSpace(request.Approver))
        {
            approverUserId = await _dbContext.Users.Where(x => x.Upn == request.Approver).Select(x => x.UserId).FirstOrDefaultAsync(cancellationToken);
        }

        if (approverUserId == Guid.Empty)
        {
            return BadRequest(new { message = "Approver user could not be resolved." });
        }

        var approverExists = await _dbContext.Users.AnyAsync(x => x.UserId == approverUserId, cancellationToken);
        if (!approverExists)
        {
            return BadRequest(new { message = "Approver user does not exist." });
        }

        var approval = new ChangeApproval
        {
            ChangeApprovalId = Guid.NewGuid(),
            ChangeRequestId = changeId,
            ApproverUserId = approverUserId,
            ApprovalStatusId = 1,
            Comments = request.Comments
        };

        var created = await _approvalService.CreateApprovalAsync(approval, cancellationToken);
        return Ok(ToResponse(created));
    }

    [HttpPost("{approvalId:guid}/decision")]
    public async Task<ActionResult<object>> DecideApproval(Guid changeId, Guid approvalId, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        var changeExists = await _dbContext.ChangeRequests.AnyAsync(x => x.ChangeRequestId == changeId, cancellationToken);
        if (!changeExists)
        {
            return NotFound(new { message = "Change request not found." });
        }

        var approval = await _approvalService.RecordDecisionAsync(changeId, approvalId, request.ApprovalStatusId, request.Comments, cancellationToken);
        return approval is null ? NotFound(new { message = "Approval record not found." }) : Ok(ToResponse(approval));
    }

    private static object ToResponse(ChangeApproval approval) => new
    {
        id = approval.ChangeApprovalId,
        changeRequestId = approval.ChangeRequestId,
        approver = approval.ApproverUser?.DisplayName ?? approval.ApproverUser?.Upn ?? string.Empty,
        status = approval.ApprovalStatus?.Name ?? "Pending",
        comment = approval.Comments,
        decisionAt = approval.ApprovedAt
    };
}
