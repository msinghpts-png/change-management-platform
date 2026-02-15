namespace ChangeManagement.Api.DTOs;

public class AttachmentDto
{
    public Guid ChangeAttachmentId { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
