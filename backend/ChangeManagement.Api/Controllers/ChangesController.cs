using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Extensions;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;
    private readonly IChangeWorkflowService _workflow;
    private readonly IWorkflowService _workflowService;
    private readonly ChangeManagementDbContext _dbContext;
    private readonly ILogger<ChangesController> _logger;
    private readonly IHostEnvironment _environment;

    public ChangesController(
        IChangeService changeService,
        IChangeWorkflowService workflow,
        IWorkflowService workflowService,
        ChangeManagementDbContext dbContext,
        ILogger<ChangesController> logger,
        IHostEnvironment environment)
    {
        _changeService = changeService;
        _workflow = workflow;
        _workflowService = workflowService;
        _dbContext = dbContext;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeRequestDto>>> GetChanges([FromQuery] Guid? requestedByUserId, CancellationToken cancellationToken)
    {
        var items = await _changeService.GetAllAsync(cancellationToken);
        if (requestedByUserId.HasValue && requestedByUserId.Value != Guid.Empty)
        {
            items = items.Where(x => x.RequestedByUserId == requestedByUserId.Value).ToList();
        }

        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> GetChangeById(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var changeId))
        {
            return BadRequest(new { message = "Invalid change request id." });
        }

        try
        {
            var entity = await _dbContext.ChangeRequests
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.ChangeType)
                .Include(x => x.Priority)
                .Include(x => x.Status)
                .Include(x => x.RiskLevel)
                .Include(x => x.RequestedByUser)
                .Include(x => x.AssignedToUser)
                .Include(x => x.ChangeApprovers)
                    .ThenInclude(x => x.ApproverUser)
                .Include(x => x.ChangeAttachments)
                .FirstOrDefaultAsync(x => x.ChangeRequestId == changeId, cancellationToken);

            var change = entity?.ToDto();

            if (change is null)
            {
                return NotFound();
            }

            return Ok(change);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error loading change {ChangeRequestId}", changeId);
            return Problem(
                title: "Unable to load change request.",
                detail: _environment.IsDevelopment() ? ex.Message : null,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateChange([FromBody] ChangeCreateDto request, CancellationToken cancellationToken)
    {
        if (request.ChangeRequestId.HasValue && request.ChangeRequestId.Value != Guid.Empty)
        {
            return BadRequest(new { message = "ChangeRequestId must not be supplied by client." });
        }

        try
        {
            var created = await _workflowService.SaveDraftAsync(request, User, cancellationToken);
            return Ok(new { changeRequestId = created.ChangeRequestId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> UpdateChange(string id, [FromBody] ChangeUpdateDto request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var changeId))
        {
            return BadRequest(new { message = "Invalid change request id." });
        }

        try
        {
            var updated = await _workflowService.SaveDraftAsync(changeId, request, User, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(ToDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ChangeRequestDto>> SubmitForApproval(string id, [FromBody] SubmitChangeRequestDto? request, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        try
        {
            var updated = await _workflowService.SubmitAsync(changeId, request, User, cancellationToken);
            if (updated is null)
            {
                return BadRequest(new { message = "Submit action is not allowed." });
            }

            return Ok(ToDto(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ChangeRequestDto>> Approve(string id, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.ApproveAsync(changeId, ResolveActorUserId(), request.Comments, cancellationToken);
        return updated is null ? BadRequest(new { message = "Approval action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ChangeRequestDto>> Reject(string id, [FromBody] ApprovalDecisionDto request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.RejectAsync(changeId, ResolveActorUserId(), request.Comments, cancellationToken);
        return updated is null ? BadRequest(new { message = "Rejection action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpPost("{id}/revert-to-draft")]
    public async Task<ActionResult<ChangeRequestDto>> RevertToDraft(string id, [FromBody] WorkflowActionRequestDto? request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.RevertToDraftAsync(changeId, ResolveActorUserId(), request?.Reason, cancellationToken);
        return updated is null ? BadRequest(new { message = "Revert action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<ChangeRequestDto>> Start(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.StartAsync(changeId, ResolveActorUserId(), User.IsInRole("Admin"), cancellationToken);
        return updated is null ? BadRequest(new { message = "Start action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ChangeRequestDto>> Complete(string id, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.CompleteAsync(changeId, ResolveActorUserId(), User.IsInRole("Admin"), cancellationToken);
        return updated is null ? BadRequest(new { message = "Complete action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<ChangeRequestDto>> Close(string id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("CAB") && !User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.CloseAsync(changeId, ResolveActorUserId(), cancellationToken);
        return updated is null ? BadRequest(new { message = "Close action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ChangeRequestDto>> Cancel(string id, [FromBody] WorkflowActionRequestDto? request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.CancelAsync(changeId, ResolveActorUserId(), request?.Reason, cancellationToken);
        return updated is null ? BadRequest(new { message = "Cancel action is not allowed." }) : Ok(ToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ChangeRequestDto>> Delete(string id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        if (!TryParseId(id, out var changeId, out var badRequest)) return badRequest;
        var updated = await _workflow.SoftDeleteAsync(changeId, ResolveActorUserId(), reason, cancellationToken);
        return updated is null ? NotFound() : Ok(ToDto(updated));
    }


    private bool TryParseId(string id, out Guid changeId, out ActionResult badRequest)
    {
        if (!Guid.TryParse(id, out changeId))
        {
            badRequest = BadRequest(new { message = "Invalid change request id." });
            return false;
        }

        badRequest = null!;
        return true;
    }

    private Guid ResolveActorUserId()
    {
        var actor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(actor, out var actorUserId) && actorUserId != Guid.Empty)
        {
            return actorUserId;
        }

        throw new UnauthorizedAccessException("Authenticated actor is required.");
    }

    private static ChangeRequestDto ToDto(ChangeRequest change) => change.ToDto();
}
