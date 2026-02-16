using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IAttachmentService
{
    Task<List<ChangeAttachment>> GetForChangeAsync(Guid changeId, CancellationToken cancellationToken);
    Task<ChangeAttachment?> GetAsync(Guid attachmentId, CancellationToken cancellationToken);
    Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, Guid uploadedBy, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken);
    Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken);
}

public class AttachmentService : IAttachmentService
{
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

    public async Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(Guid changeId, IFormFile file, Guid uploadedBy, CancellationToken cancellationToken)
    {
        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is null)
        {
            return (null, "Change request not found.");
        }

        var rootPath = Path.Combine(_environment.ContentRootPath, "App_Data", "attachments", changeId.ToString("N"));
        Directory.CreateDirectory(rootPath);

        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
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
            UploadedBy = uploadedBy
        };

        var created = await _attachmentRepository.CreateAsync(entity, cancellationToken);
        await _audit.LogAsync(5, uploadedBy, "system@local", "cm", "ChangeAttachment", created.ChangeAttachmentId, change.ChangeNumber.ToString(), "Upload", created.FileName, cancellationToken);
        return (created, null);
    }

    public Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken) => _attachmentRepository.DeleteAsync(attachmentId, cancellationToken);

    public async Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken)
    {
        if (!File.Exists(attachment.FileUrl)) return null;
        return await File.ReadAllBytesAsync(attachment.FileUrl, cancellationToken);
    }
}
