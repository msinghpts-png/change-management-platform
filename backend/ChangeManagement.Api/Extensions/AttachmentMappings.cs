using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;

namespace ChangeManagement.Api.Extensions;

public static class AttachmentMappings
{
    public static AttachmentDto ToDto(this ChangeAttachment attachment)
    {
        return new AttachmentDto
        {
            Id = attachment.ChangeAttachmentId,
            ChangeAttachmentId = attachment.ChangeAttachmentId,
            ChangeRequestId = attachment.ChangeRequestId,
            FileName = attachment.FileName,
            FileUrl = attachment.FileUrl,
            FileSizeBytes = attachment.FileSizeBytes,
            UploadedAt = attachment.UploadedAt
        };
    }
}
