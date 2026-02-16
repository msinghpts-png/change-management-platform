using ChangeManagement.Api.DTOs.Admin;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AdminController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("audit")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAudit(CancellationToken cancellationToken)
    {
        var logs = await _auditService.ListAsync(cancellationToken);
        return Ok(logs.Select(x => new AuditLogDto
        {
            AuditLogId = x.AuditLogId,
            ChangeId = x.ChangeId,
            ActorUserId = x.ActorUserId,
            Action = x.Action,
            Details = x.Details,
            CreatedAt = x.CreatedAt
        }));
    }
}
