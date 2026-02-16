using ChangeManagement.Api.Data;
using ChangeManagement.Api.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/admin/database")]
public class AdminController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public AdminController(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    [HttpGet("status")]
    public async Task<ActionResult<DatabaseStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var pending = (await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        var csb = new SqlConnectionStringBuilder(_dbContext.Database.GetConnectionString());

        return Ok(new DatabaseStatusDto
        {
            DatabaseName = csb.InitialCatalog,
            TotalChanges = await _dbContext.ChangeRequests.CountAsync(cancellationToken),
            TotalApprovals = await _dbContext.ChangeApprovals.CountAsync(cancellationToken),
            TotalAttachments = await _dbContext.ChangeAttachments.CountAsync(cancellationToken),
            HasPendingMigrations = pending.Count > 0,
            PendingMigrations = pending
        });
    }

    [HttpPost("migrate")]
    public async Task<IActionResult> RunMigrations(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        return Ok(new { message = "Migrations applied successfully." });
    }
}
