using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IAttachmentService
{
    Task<List<ChangeAttachment>> GetForChangeAsync(Guid changeId, CancellationToken cancellationToken);
    Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, Guid uploadedByUserId, CancellationToken cancellationToken);
}

public class AttachmentService : IAttachmentService
{
    private static readonly string[] AllowedTypes = ["application/pdf", "image/png", "image/jpeg", "text/plain"];
    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly IChangeAttachmentRepository _attachmentRepository;
    private readonly IChangeRepository _changeRepository;
    private readonly IAuditService _audit;

    public AttachmentService(IChangeAttachmentRepository attachmentRepository, IChangeRepository changeRepository, IAuditService audit)
    {
        _attachmentRepository = attachmentRepository;
        _changeRepository = changeRepository;
        _audit = audit;
    }

    public Task<List<ChangeAttachment>> GetForChangeAsync(Guid changeId, CancellationToken cancellationToken) => _attachmentRepository.GetByChangeIdAsync(changeId, cancellationToken);

    public async Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, Guid uploadedByUserId, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null) return (null, "Change request not found.");

        if (file.Length <= 0 || file.Length > MaxBytes) return (null, "Invalid file size.");
        if (!AllowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)) return (null, "Invalid file type.");

        var rootPath = Path.Combine("/data/attachments", changeId.ToString());
        Directory.CreateDirectory(rootPath);

        var attachmentId = Guid.NewGuid();
        var fileName = $"{attachmentId}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(rootPath, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var attachment = new ChangeAttachment
        {
            ChangeAttachmentId = attachmentId,
            ChangeId = changeId,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            FilePath = fullPath,
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };

        var created = await _attachmentRepository.CreateAsync(attachment, cancellationToken);
        await _audit.LogAsync(uploadedByUserId, "AttachmentUploaded", created.FileName, changeId, cancellationToken);
        return (created, null);
    }
}
