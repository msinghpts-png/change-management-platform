using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public DashboardController(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var response = new
        {
            totalChanges = await _dbContext.ChangeRequests.CountAsync(cancellationToken),
            pendingApprovals = await _dbContext.ChangeRequests.CountAsync(change => change.Status == ChangeStatus.Submitted, cancellationToken),
            scheduledThisWeek = await _dbContext.ChangeRequests.CountAsync(change => change.ImplementationDate >= startOfWeek && change.ImplementationDate <= now.AddDays(7), cancellationToken),
            completedThisMonth = await _dbContext.ChangeRequests.CountAsync(change => change.Status == ChangeStatus.Completed && change.CompletedDate >= startOfMonth, cancellationToken),
            inImplementation = await _dbContext.ChangeRequests.CountAsync(change => change.Status == ChangeStatus.InImplementation, cancellationToken),
            emergencyChanges = await _dbContext.ChangeRequests.CountAsync(change => change.ChangeType == ChangeType.Emergency, cancellationToken)
        };

        return Ok(response);
    }
}
