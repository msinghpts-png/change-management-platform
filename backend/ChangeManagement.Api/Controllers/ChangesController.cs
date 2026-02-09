using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes")]
public class ChangesController : ControllerBase
{
    private readonly IChangeService _changeService;

    public ChangesController(IChangeService changeService)
    {
        _changeService = changeService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ChangeRequestDto>> GetChanges()
    {
        var changes = _changeService.GetAll();

        return Ok(changes.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ChangeRequestDto> GetChangeById(Guid id)
    {
        var change = _changeService.GetById(id);
        if (change is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(change));
    }

    [HttpPost]
    public ActionResult<ChangeRequestDto> CreateChange([FromBody] ChangeCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var created = new ChangeRequest
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = ChangeStatus.Draft,
            Priority = request.Priority,
            RiskLevel = request.RiskLevel,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            CreatedAt = DateTime.UtcNow
        };

        var stored = _changeService.Create(created);

        return CreatedAtAction(nameof(GetChangeById), new { id = stored.Id }, MapToDto(stored));
    }

    [HttpPut("{id:guid}")]
    public ActionResult<ChangeRequestDto> UpdateChange(Guid id, [FromBody] ChangeUpdateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var updated = new ChangeRequest
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Status = Enum.TryParse<ChangeStatus>(request.Status, true, out var status)
                ? status
                : ChangeStatus.Draft,
            Priority = request.Priority,
            RiskLevel = request.RiskLevel,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        var stored = _changeService.Update(updated);
        if (stored is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(stored));
    }

    private static ChangeRequestDto MapToDto(ChangeRequest change)
    {
        return new ChangeRequestDto
        {
            Id = change.Id,
            Title = change.Title,
            Description = change.Description,
            Status = change.Status.ToString(),
            Priority = change.Priority,
            RiskLevel = change.RiskLevel,
            PlannedStart = change.PlannedStart,
            PlannedEnd = change.PlannedEnd,
            CreatedAt = change.CreatedAt,
            UpdatedAt = change.UpdatedAt
        };
    }
}
