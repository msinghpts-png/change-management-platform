using ChangeManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/templates")]
public class AdminTemplatesController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public AdminTemplatesController(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var changeTypeLookup = await _dbContext.ChangeTypes.AsNoTracking()
            .ToDictionaryAsync(x => x.ChangeTypeId, x => x.Name, cancellationToken);
        var riskLookup = await _dbContext.RiskLevels.AsNoTracking()
            .ToDictionaryAsync(x => x.RiskLevelId, x => x.Name, cancellationToken);

        var items = await _dbContext.ChangeTemplates
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(x => new
        {
            x.TemplateId,
            x.Name,
            x.Description,
            x.ImplementationSteps,
            x.BackoutPlan,
            x.ServiceSystem,
            x.Category,
            x.Environment,
            x.ChangeTypeId,
            changeTypeName = x.ChangeTypeId.HasValue && changeTypeLookup.TryGetValue(x.ChangeTypeId.Value, out var changeTypeName) ? changeTypeName : null,
            x.RiskLevelId,
            riskLevelName = x.RiskLevelId.HasValue && riskLookup.TryGetValue(x.RiskLevelId.Value, out var riskLevelName) ? riskLevelName : null,
            x.IsActive,
            x.CreatedAt,
            x.CreatedBy,
            createdByUpn = x.CreatedByUser != null ? x.CreatedByUser.Upn : null,
            createdByDisplayName = x.CreatedByUser != null ? x.CreatedByUser.DisplayName : null
        }));
    }
}
