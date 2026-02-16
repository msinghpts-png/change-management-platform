using System.Security.Claims;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface ITemplateService
{
    Task<List<ChangeTemplate>> ListAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<ChangeTemplate?> GetAsync(Guid templateId, CancellationToken cancellationToken);
    Task<ChangeTemplate> CreateAsync(ChangeTemplateUpsertDto request, CancellationToken cancellationToken);
    Task<ChangeTemplate?> UpdateAsync(Guid templateId, ChangeTemplateUpsertDto request, CancellationToken cancellationToken);
    Task<bool> SoftDeleteAsync(Guid templateId, CancellationToken cancellationToken);
}

public class TemplateService : ITemplateService
{
    private readonly ChangeManagementDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditService _auditService;

    public TemplateService(ChangeManagementDbContext dbContext, IHttpContextAccessor httpContextAccessor, IAuditService auditService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _auditService = auditService;
    }

    public async Task<List<ChangeTemplate>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = _dbContext.ChangeTemplates.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<ChangeTemplate?> GetAsync(Guid templateId, CancellationToken cancellationToken)
        => _dbContext.ChangeTemplates.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);

    public async Task<ChangeTemplate> CreateAsync(ChangeTemplateUpsertDto request, CancellationToken cancellationToken)
    {
        var actorId = ResolveActorUserId();
        var entity = new ChangeTemplate
        {
            TemplateId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            ImplementationSteps = request.ImplementationSteps,
            BackoutPlan = request.BackoutPlan,
            ServiceSystem = request.ServiceSystem,
            Category = request.Category,
            Environment = request.Environment,
            BusinessJustification = request.BusinessJustification,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId,
            IsActive = request.IsActive ?? true
        };

        _dbContext.ChangeTemplates.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(6, actorId ?? Guid.Empty, ResolveActorUpn(), "cm", "ChangeTemplate", entity.TemplateId, string.Empty, "TemplateCreate", entity.Name, cancellationToken);
        return entity;
    }

    public async Task<ChangeTemplate?> UpdateAsync(Guid templateId, ChangeTemplateUpsertDto request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ChangeTemplates.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
        if (entity is null) return null;

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.ImplementationSteps = request.ImplementationSteps;
        entity.BackoutPlan = request.BackoutPlan;
        entity.ServiceSystem = request.ServiceSystem;
        entity.Category = request.Category;
        entity.Environment = request.Environment;
        entity.BusinessJustification = request.BusinessJustification;
        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        var actorId = ResolveActorUserId() ?? Guid.Empty;
        await _auditService.LogAsync(7, actorId, ResolveActorUpn(), "cm", "ChangeTemplate", entity.TemplateId, string.Empty, "TemplateUpdate", entity.Name, cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ChangeTemplates.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
        if (entity is null) return false;

        entity.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var actorId = ResolveActorUserId() ?? Guid.Empty;
        await _auditService.LogAsync(7, actorId, ResolveActorUpn(), "cm", "ChangeTemplate", entity.TemplateId, string.Empty, "TemplateUpdate", entity.Name, cancellationToken);
        return true;
    }

    private Guid? ResolveActorUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(claim, out var parsed) && parsed != Guid.Empty) return parsed;
        return null;
    }

    private string ResolveActorUpn()
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Upn)
           ?? _httpContextAccessor.HttpContext?.User.Identity?.Name
           ?? "system@local";
}
