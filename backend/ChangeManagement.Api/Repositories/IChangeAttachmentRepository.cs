using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IChangeAttachmentRepository
{
    IEnumerable<ChangeAttachment> GetByChangeId(Guid changeRequestId);
    ChangeAttachment? GetById(Guid attachmentId);
    ChangeAttachment Create(ChangeAttachment attachment);
    void Delete(ChangeAttachment attachment);
}
