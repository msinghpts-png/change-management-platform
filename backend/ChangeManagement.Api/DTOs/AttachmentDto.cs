namespace ChangeManagement.Api.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
