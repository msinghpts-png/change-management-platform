using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;

namespace ChangeManagement.Api.Extensions;

public static class AttachmentMappings
{
    public static AttachmentDto ToDto(this ChangeAttachment attachment)
    {
        return new AttachmentDto
        {
            ChangeAttachmentId = attachment.ChangeAttachmentId,
            ChangeId = attachment.ChangeId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            UploadedAt = attachment.UploadedAt
        };
    }
}
