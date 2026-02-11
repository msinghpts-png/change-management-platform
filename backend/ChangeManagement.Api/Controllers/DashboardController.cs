using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    [HttpGet]
    public IActionResult GetDashboard()
    {
        var response = new
        {
            totalChanges = 12,
            pendingApprovals = 3,
            scheduledThisWeek = 2,
            completedThisMonth = 5
        };

        return Ok(response);
    }
}
