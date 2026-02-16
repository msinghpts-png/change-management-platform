using ChangeManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/audit")]
public class AdminAuditController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public AdminAuditController(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> GetAudit([FromQuery] int? eventTypeId, [FromQuery] Guid? actorUserId, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditEvents.AsNoTracking().OrderByDescending(x => x.EventAt).AsQueryable();
        if (eventTypeId.HasValue) query = query.Where(x => x.EventTypeId == eventTypeId.Value);
        if (actorUserId.HasValue && actorUserId.Value != Guid.Empty) query = query.Where(x => x.ActorUserId == actorUserId.Value);

        var items = await query.Take(500).ToListAsync(cancellationToken);
        return Ok(items.Select(x => new
        {
            x.AuditEventId,
            x.EventTypeId,
            x.EventAt,
            x.ActorUserId,
            x.ActorUpn,
            x.EntitySchema,
            x.EntityName,
            x.EntityId,
            x.ChangeNumber,
            x.Reason,
            x.Details
        }));
    }
}
