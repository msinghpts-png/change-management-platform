using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/changes/{changeId:guid}/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService) => _approvalService = approvalService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(Guid changeId, CancellationToken cancellationToken)
    {
        var approvals = await _approvalService.GetApprovalsForChangeAsync(changeId, cancellationToken);
        return Ok(approvals.Select(a => new
        {
            a.ChangeApprovalId,
            a.ChangeId,
            a.CabUserId,
            a.IsApproved,
            a.Comments,
            a.DecisionDate
        }));
    }
}
