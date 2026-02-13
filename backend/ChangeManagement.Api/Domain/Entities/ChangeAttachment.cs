using System.ComponentModel.DataAnnotations.Schema;

namespace ChangeManagement.Api.Domain.Entities;

public class ChangeAttachment
{
    public Guid Id { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    [NotMapped]
    public long SizeBytes
    {
        get => FileSize;
        set => FileSize = value;
    }

    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
