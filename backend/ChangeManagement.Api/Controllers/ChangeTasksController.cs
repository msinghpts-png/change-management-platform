using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes/{changeId:guid}/tasks")]
public class ChangeTasksController : ControllerBase
{
    private readonly IChangeTaskService _service;
    public ChangeTasksController(IChangeTaskService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeTask>>> Get(Guid changeId, CancellationToken cancellationToken) => Ok(await _service.GetByChangeAsync(changeId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ChangeTask>> Create(Guid changeId, [FromBody] ChangeTaskCreateDto request, CancellationToken cancellationToken)
    {
        var task = new ChangeTask
        {
            ChangeTaskId = Guid.NewGuid(),
            ChangeRequestId = changeId,
            Title = request.Title,
            Description = request.Description,
            StatusId = request.StatusId,
            AssignedToUserId = request.AssignedToUserId,
            DueAt = request.DueAt
        };

        var created = await _service.CreateAsync(task, cancellationToken);
        return CreatedAtAction(nameof(Get), new { changeId }, created);
    }

    [HttpPut("{taskId:guid}")]
    public async Task<ActionResult<ChangeTask>> Update(Guid changeId, Guid taskId, [FromBody] ChangeTaskUpdateDto request, CancellationToken cancellationToken)
    {
        var task = new ChangeTask
        {
            ChangeTaskId = taskId,
            ChangeRequestId = changeId,
            Title = request.Title,
            Description = request.Description,
            StatusId = request.StatusId,
            AssignedToUserId = request.AssignedToUserId,
            DueAt = request.DueAt,
            CompletedAt = request.CompletedAt
        };

        var updated = await _service.UpdateAsync(task, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken cancellationToken)
        => await _service.DeleteAsync(taskId, cancellationToken) ? NoContent() : NotFound();
}
