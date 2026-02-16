using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/changes/{changeId:guid}/tasks")]
public class ChangeTasksController : ControllerBase
{
    private readonly IChangeTaskService _service;
    public ChangeTasksController(IChangeTaskService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(Guid changeId, CancellationToken cancellationToken)
    {
        var tasks = await _service.GetByChangeAsync(changeId, cancellationToken);
        return Ok(tasks.Select(t => new { t.ChangeTaskId, t.ChangeId, t.Title, t.Description, t.AssignedToUserId, t.DueDate, t.CompletedDate }));
    }
}
