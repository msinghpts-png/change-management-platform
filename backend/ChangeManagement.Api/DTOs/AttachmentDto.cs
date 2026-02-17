namespace ChangeManagement.Api.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public Guid ChangeAttachmentId { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid? UploadedBy { get; set; }
}
