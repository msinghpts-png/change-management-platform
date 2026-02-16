using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeAttachmentRepository : IChangeAttachmentRepository
{
    private readonly ChangeManagementDbContext _dbContext;
    public ChangeAttachmentRepository(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public Task<List<ChangeAttachment>> GetByChangeIdAsync(Guid changeId, CancellationToken cancellationToken) =>
        _dbContext.ChangeAttachments.Include(a => a.UploadedByUser).Where(a => a.ChangeId == changeId).ToListAsync(cancellationToken);

    public Task<ChangeAttachment?> GetByIdAsync(Guid attachmentId, CancellationToken cancellationToken) =>
        _dbContext.ChangeAttachments.Include(a => a.UploadedByUser).FirstOrDefaultAsync(a => a.ChangeAttachmentId == attachmentId, cancellationToken);

    public async Task<ChangeAttachment> CreateAsync(ChangeAttachment attachment, CancellationToken cancellationToken)
    {
        _dbContext.ChangeAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(attachment.ChangeAttachmentId, cancellationToken) ?? attachment;
    }

    public async Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(attachmentId, cancellationToken);
        if (entity is null) return false;
        _dbContext.ChangeAttachments.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
