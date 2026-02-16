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
    public async Task<ActionResult<IEnumerable<ChangeApproval>>> GetApprovals(Guid changeId, CancellationToken cancellationToken)
        => Ok(await _approvalService.GetApprovalsForChangeAsync(changeId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ChangeApproval>> CreateApproval(Guid changeId, [FromBody] ApprovalCreateDto request, CancellationToken cancellationToken)
    {
        var approverUserId = request.ApproverUserId ?? Guid.Empty;
        if (approverUserId == Guid.Empty && !string.IsNullOrWhiteSpace(request.Approver))
        {
            approverUserId = await _dbContext.Users.Where(x => x.Upn == request.Approver).Select(x => x.UserId).FirstOrDefaultAsync(cancellationToken);
        }

        if (approverUserId == Guid.Empty)
        {
            return BadRequest("Approver user could not be resolved.");
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
        return CreatedAtAction(nameof(GetApprovals), new { changeId }, created);
    }

    [HttpPost("{approvalId:guid}/decision")]
    public async Task<ActionResult<ChangeApproval>> DecideApproval(Guid changeId, Guid approvalId, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        var approval = await _approvalService.RecordDecisionAsync(changeId, approvalId, request.ApprovalStatusId, request.Comments, cancellationToken);
        return approval is null ? NotFound() : Ok(approval);
    }
}
