using System.Security.Claims;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public AttachmentService(IChangeAttachmentRepository attachmentRepository, IChangeRepository changeRepository, IAuditService audit, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _attachmentRepository = attachmentRepository;
        _changeRepository = changeRepository;
        _audit = audit;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
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

        var rootPath = Path.Combine(ResolveAttachmentRoot(), changeId.ToString());
        Directory.CreateDirectory(rootPath);

        var fileId = Guid.NewGuid();
        var safeName = Path.GetFileName(file.FileName);
        var storedFileName = $"{fileId}_{safeName}";
        var storedPath = Path.Combine(rootPath, storedFileName);

        await using var stream = File.Create(storedPath);
        await file.CopyToAsync(stream, cancellationToken);

        var resolvedUploader = uploadedBy;
        if (!resolvedUploader.HasValue || resolvedUploader == Guid.Empty)
        {
            resolvedUploader = change.RequestedByUserId != Guid.Empty ? change.RequestedByUserId : change.CreatedBy;
        }

        var entity = new ChangeAttachment
        {
            ChangeAttachmentId = fileId,
            ChangeRequestId = changeId,
            FileName = Path.GetFileName(file.FileName),
            FileUrl = storedPath,
            FilePath = storedPath,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = resolvedUploader,
            FileSizeBytes = file.Length
        };

        var created = await _attachmentRepository.CreateAsync(entity, cancellationToken);
        var actor = created.UploadedBy ?? change.CreatedBy;
        await _audit.LogAsync(5, actor, ResolveActorUpn(), "cm", "ChangeAttachment", created.ChangeAttachmentId, change.ChangeNumber.ToString(), "AttachmentUpload", created.FileName, cancellationToken);
        return (created, null);
    }


    private string ResolveAttachmentRoot()
    {
        var configured = _configuration["AttachmentStorage:RootPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var envPath = Environment.GetEnvironmentVariable("ATTACHMENT_STORAGE_ROOT");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return envPath;
        }

        var dockerVolumePath = "/data/attachments";
        if (Directory.Exists(dockerVolumePath))
        {
            return dockerVolumePath;
        }

        return Path.Combine(_environment.ContentRootPath, "attachments");
    }

    private string ResolveActorUpn()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.Upn)
               ?? user?.FindFirstValue(ClaimTypes.Email)
               ?? user?.Identity?.Name
               ?? "unknown@local";
    }

    public Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken) => _attachmentRepository.DeleteAsync(attachmentId, cancellationToken);

    public async Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrWhiteSpace(attachment.FilePath) ? attachment.FileUrl : attachment.FilePath;
        if (!File.Exists(path)) return null;
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }
}
