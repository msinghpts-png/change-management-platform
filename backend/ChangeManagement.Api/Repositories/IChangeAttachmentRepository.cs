using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IChangeAttachmentRepository
{
    Task<List<ChangeAttachment>> GetByChangeIdAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeAttachment?> GetByIdAsync(Guid attachmentId, CancellationToken cancellationToken);
    Task<ChangeAttachment> CreateAsync(ChangeAttachment attachment, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken);
}
