using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IAttachmentService
{
    Task<List<ChangeAttachment>> GetForChangeAsync(Guid changeId, CancellationToken cancellationToken);
    Task<ChangeAttachment?> GetAsync(Guid attachmentId, CancellationToken cancellationToken);
    Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, Guid? uploadedBy, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken);
    Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken);
}

public class AttachmentService : IAttachmentService
{
    private const long MaxFileBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".txt" };
    private readonly IChangeAttachmentRepository _attachmentRepository;
    private readonly IChangeRepository _changeRepository;
    private readonly IAuditService _audit;
    private readonly IWebHostEnvironment _environment;

    public AttachmentService(IChangeAttachmentRepository attachmentRepository, IChangeRepository changeRepository, IAuditService audit, IWebHostEnvironment environment)
    {
        _attachmentRepository = attachmentRepository;
        _changeRepository = changeRepository;
        _audit = audit;
        _environment = environment;
    }

    public Task<List<ChangeAttachment>> GetForChangeAsync(Guid changeId, CancellationToken cancellationToken) => _attachmentRepository.GetByChangeIdAsync(changeId, cancellationToken);
    public Task<ChangeAttachment?> GetAsync(Guid attachmentId, CancellationToken cancellationToken) => _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

    public async Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, Guid? uploadedBy, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null)
        {
            return (null, "Change request not found.");
        }

        if (file is null || file.Length == 0) return (null, "No file uploaded.");
        if (file.Length > MaxFileBytes) return (null, "File exceeds 5MB limit.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return (null, "File type is not allowed.");
        }

        var rootPath = Path.Combine(_environment.ContentRootPath, "attachments", changeId.ToString());
        Directory.CreateDirectory(rootPath);

        var fileId = Guid.NewGuid();
        var storedPath = Path.Combine(rootPath, $"{fileId:N}{extension}");

        await using var stream = File.Create(storedPath);
        await file.CopyToAsync(stream, cancellationToken);

        var entity = new ChangeAttachment
        {
            ChangeAttachmentId = fileId,
            ChangeRequestId = changeId,
            FileName = Path.GetFileName(file.FileName),
            FileUrl = storedPath,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy == Guid.Empty ? change.CreatedBy : uploadedBy,
            FileSizeBytes = file.Length
        };

        var created = await _attachmentRepository.CreateAsync(entity, cancellationToken);
        var actor = created.UploadedBy ?? change.CreatedBy;
        await _audit.LogAsync(5, actor, "system@local", "cm", "ChangeAttachment", created.ChangeAttachmentId, change.ChangeNumber.ToString(), "Upload", created.FileName, cancellationToken);
        return (created, null);
    }

    public Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken) => _attachmentRepository.DeleteAsync(attachmentId, cancellationToken);

    public async Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken)
    {
        if (!File.Exists(attachment.FileUrl)) return null;
        return await File.ReadAllBytesAsync(attachment.FileUrl, cancellationToken);
    }
}
