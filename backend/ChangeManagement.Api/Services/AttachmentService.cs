using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IAttachmentService
{
    IEnumerable<ChangeAttachment> GetForChange(Guid changeId);
    ChangeAttachment? Get(Guid attachmentId);
    Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken);
    Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken);
}

public class AttachmentService : IAttachmentService
{
    private const long MaxBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".txt", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"
    };

    private readonly IChangeAttachmentRepository _attachmentRepository;
    private readonly IChangeRepository _changeRepository;
    private const string UploadRoot = "/app/uploads";

    public AttachmentService(
        IChangeAttachmentRepository attachmentRepository,
        IChangeRepository changeRepository)
    {
        _attachmentRepository = attachmentRepository;
        _changeRepository = changeRepository;
    }

    public IEnumerable<ChangeAttachment> GetForChange(Guid changeId) => _attachmentRepository.GetByChangeId(changeId);

    public ChangeAttachment? Get(Guid attachmentId) => _attachmentRepository.GetById(attachmentId);

    public async Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, CancellationToken cancellationToken)
    {
        if (_changeRepository.GetById(changeId) is null)
        {
            return (null, "Change request not found.");
        }

        if (file.Length <= 0)
        {
            return (null, "Attachment file is empty.");
        }

        if (file.Length > MaxBytes)
        {
            return (null, "Attachment exceeds maximum size of 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return (null, "Attachment type is not allowed.");
        }

        var rootPath = Path.Combine(UploadRoot, changeId.ToString("N"));
        Directory.CreateDirectory(rootPath);

        var fileId = Guid.NewGuid();
        var storedFileName = $"{fileId:N}{extension}";
        var storedPath = Path.Combine(rootPath, storedFileName);

        await using (var stream = File.Create(storedPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var entity = new ChangeAttachment
        {
            Id = fileId,
            ChangeRequestId = changeId,
            FileName = Path.GetFileName(file.FileName),
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            SizeBytes = file.Length,
            StoragePath = storedPath,
            UploadedAt = DateTime.UtcNow
        };

        var stored = _attachmentRepository.Create(entity);
        return (stored, null);
    }

    public async Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = _attachmentRepository.GetById(attachmentId);
        if (attachment is null)
        {
            return false;
        }

        _attachmentRepository.Delete(attachment);
        if (File.Exists(attachment.StoragePath))
        {
            await Task.Run(() => File.Delete(attachment.StoragePath), cancellationToken);
        }

        return true;
    }

    public async Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken)
    {
        if (!File.Exists(attachment.StoragePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(attachment.StoragePath, cancellationToken);
    }
}
