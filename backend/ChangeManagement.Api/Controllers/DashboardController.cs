using ChangeManagement.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public DashboardController(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var response = new
        {
            totalChanges = await _dbContext.ChangeRequests.CountAsync(),
            submittedChanges = await _dbContext.ChangeRequests.CountAsync(change => change.StatusId == 2),
            scheduledThisWeek = await _dbContext.ChangeRequests.CountAsync(change => change.PlannedStart >= startOfWeek && change.PlannedStart <= now.AddDays(7)),
            completedThisMonth = await _dbContext.ChangeRequests.CountAsync(change => change.StatusId == 5 && change.UpdatedAt >= startOfMonth)
        };

        return Ok(response);
    }
}
