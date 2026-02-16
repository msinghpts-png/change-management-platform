namespace ChangeManagement.Api.Domain.Entities;

public class ChangeAttachment
{
    public Guid ChangeAttachmentId { get; set; }
    public Guid ChangeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid UploadedByUserId { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? UploadedByUser { get; set; }
}
