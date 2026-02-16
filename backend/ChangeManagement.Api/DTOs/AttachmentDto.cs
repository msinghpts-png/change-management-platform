namespace ChangeManagement.Api.DTOs;

public class AttachmentDto
{
    public Guid ChangeAttachmentId { get; set; }
    public Guid ChangeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
