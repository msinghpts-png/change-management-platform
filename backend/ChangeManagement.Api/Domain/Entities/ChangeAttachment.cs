namespace ChangeManagement.Api.Domain.Entities;

public class ChangeAttachment
{
    public Guid Id { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
