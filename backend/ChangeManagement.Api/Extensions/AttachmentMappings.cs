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
            FileUrl = $"/api/changes/{attachment.ChangeRequestId}/attachments/{attachment.ChangeAttachmentId}/download",
            FilePath = string.IsNullOrWhiteSpace(attachment.FilePath) ? string.Empty : attachment.FilePath,
            FileSizeBytes = attachment.FileSizeBytes,
            UploadedAt = attachment.UploadedAt,
            UploadedBy = attachment.UploadedBy
        };
    }
}
