using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;

namespace ChangeManagement.Api.Extensions;

public static class AttachmentMappings
{
    public static AttachmentDto ToDto(this ChangeAttachment attachment)
    {
        return new AttachmentDto
        {
            Id = attachment.Id,
            ChangeRequestId = attachment.ChangeRequestId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            SizeBytes = attachment.SizeBytes,
            UploadedAt = attachment.UploadedAt
        };
    }
}
