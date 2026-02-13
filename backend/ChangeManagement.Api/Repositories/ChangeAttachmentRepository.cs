using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public class ChangeAttachmentRepository : IChangeAttachmentRepository
{
    private readonly ChangeManagementDbContext _dbContext;

    public ChangeAttachmentRepository(ChangeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<ChangeAttachment> GetByChangeId(Guid changeRequestId)
    {
        return _dbContext.ChangeAttachments
            .Where(attachment => attachment.ChangeRequestId == changeRequestId)
            .OrderByDescending(attachment => attachment.UploadedAt)
            .ToList();
    }

    public ChangeAttachment? GetById(Guid attachmentId)
    {
        return _dbContext.ChangeAttachments.FirstOrDefault(attachment => attachment.Id == attachmentId);
    }

    public ChangeAttachment Create(ChangeAttachment attachment)
    {
        _dbContext.ChangeAttachments.Add(attachment);
        _dbContext.SaveChanges();
        return attachment;
    }

    public void Delete(ChangeAttachment attachment)
    {
        _dbContext.ChangeAttachments.Remove(attachment);
        _dbContext.SaveChanges();
    }
}
