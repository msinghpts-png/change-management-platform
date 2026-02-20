using System.Security.Claims;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface ITemplateService
{
    Task<IReadOnlyList<ChangeTemplate>> ListAsync(bool includeInactive, CancellationToken ct);
    Task<ChangeTemplate?> GetAsync(Guid id, CancellationToken ct);
    Task<ChangeTemplate> CreateAsync(ChangeTemplateUpsertDto request, CancellationToken ct);
    Task<ChangeTemplate?> UpdateAsync(Guid id, ChangeTemplateUpsertDto request, CancellationToken ct);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);
}

public class TemplateService : ITemplateService
{
    private static readonly Guid DefaultActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ChangeManagementDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplateService(ChangeManagementDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<ChangeTemplate>> ListAsync(bool includeInactive, CancellationToken ct)
    {
        var query = _context.ChangeTemplates.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public Task<ChangeTemplate?> GetAsync(Guid id, CancellationToken ct)
        => _context.ChangeTemplates.FirstOrDefaultAsync(t => t.TemplateId == id, ct);

    public async Task<ChangeTemplate> CreateAsync(ChangeTemplateUpsertDto request, CancellationToken ct)
    {
        var template = new ChangeTemplate
        {
            TemplateId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ImplementationSteps = request.ImplementationSteps,
            BackoutPlan = request.BackoutPlan,
            ServiceSystem = request.ServiceSystem,
            Category = request.Category,
            Environment = request.Environment,
            BusinessJustification = request.BusinessJustification,
            ChangeTypeId = request.ChangeTypeId,
            RiskLevelId = request.RiskLevelId,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = ResolveActorUserId()
        };

        _context.ChangeTemplates.Add(template);
        await _context.SaveChangesAsync(ct);
        return template;
    }

    public async Task<ChangeTemplate?> UpdateAsync(Guid id, ChangeTemplateUpsertDto request, CancellationToken ct)
    {
        var template = await _context.ChangeTemplates.FirstOrDefaultAsync(t => t.TemplateId == id, ct);
        if (template is null)
        {
            return null;
        }

        template.Name = request.Name;
        template.Description = request.Description;
        template.ImplementationSteps = request.ImplementationSteps;
        template.BackoutPlan = request.BackoutPlan;
        template.ServiceSystem = request.ServiceSystem;
        template.Category = request.Category;
        template.Environment = request.Environment;
        template.BusinessJustification = request.BusinessJustification;
        template.ChangeTypeId = request.ChangeTypeId;
        template.RiskLevelId = request.RiskLevelId;
        template.IsActive = request.IsActive ?? template.IsActive;

        await _context.SaveChangesAsync(ct);
        return template;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct)
    {
        var template = await _context.ChangeTemplates.FirstOrDefaultAsync(t => t.TemplateId == id, ct);
        if (template is null)
        {
            return false;
        }

        template.IsActive = false;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private Guid ResolveActorUserId()
    {
        var actorClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(actorClaim, out var actorUserId) && actorUserId != Guid.Empty)
        {
            return actorUserId;
        }

        return DefaultActorUserId;
    }
}
