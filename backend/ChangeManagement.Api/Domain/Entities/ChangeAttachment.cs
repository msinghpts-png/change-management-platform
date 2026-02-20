namespace ChangeManagement.Api.Domain.Entities;

public class ChangeAttachment
{
    public Guid ChangeAttachmentId { get; set; }
    public Guid ChangeRequestId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; }
    public Guid? UploadedBy { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? UploadedByUser { get; set; }
}
