using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public class TemplateService : ITemplateService
{
    private readonly ChangeManagementDbContext _context;

    public TemplateService(ChangeManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ChangeTemplate> CreateAsync(ChangeTemplate template, Guid createdBy, CancellationToken ct)
    {
        template.TemplateId = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        template.CreatedBy = createdBy;

        _context.ChangeTemplates.Add(template);
        await _context.SaveChangesAsync(ct);

        return template;
    }

    public async Task<List<ChangeTemplate>> GetAllAsync(CancellationToken ct)
    {
        return await _context.ChangeTemplates
            .Where(t => t.IsActive)
            .ToListAsync(ct);
    }

    public async Task<ChangeTemplate?> GetAsync(Guid id, CancellationToken ct)
    {
        return await _context.ChangeTemplates
            .FirstOrDefaultAsync(t => t.TemplateId == id, ct);
    }
}
