using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.DTOs.Admin;

public class DatabaseBackupDto
{
    public List<ChangeRequest> ChangeRequests { get; set; } = [];
    public List<ChangeApproval> ChangeApprovals { get; set; } = [];
    public List<AttachmentBackupItemDto> ChangeAttachments { get; set; } = [];
}

public class AttachmentBackupItemDto
{
    public Guid Id { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? ContentBase64 { get; set; }
}
