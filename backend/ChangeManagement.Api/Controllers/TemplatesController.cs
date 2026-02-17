using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService) => _templateService = templateService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeTemplateDto>>> List([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var items = await _templateService.ListAsync(includeInactive, cancellationToken);
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChangeTemplateDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await _templateService.GetAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<ChangeTemplateDto>> Create([FromBody] ChangeTemplateUpsertDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Template name is required.");

        var created = await _templateService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.TemplateId }, ToDto(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChangeTemplateDto>> Update(Guid id, [FromBody] ChangeTemplateUpsertDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Template name is required.");

        var updated = await _templateService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(ToDto(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _templateService.SoftDeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static ChangeTemplateDto ToDto(ChangeTemplate entity) => new()
    {
        TemplateId = entity.TemplateId,
        Name = entity.Name,
        Description = entity.Description,
        ImplementationSteps = entity.ImplementationSteps,
        BackoutPlan = entity.BackoutPlan,
        ServiceSystem = entity.ServiceSystem,
        Category = entity.Category,
        Environment = entity.Environment,
        BusinessJustification = entity.BusinessJustification,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        IsActive = entity.IsActive
    };
}
