using System.Security.Claims;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;
using Microsoft.EntityFrameworkCore;

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
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".txt"
    };

    private readonly IChangeAttachmentRepository _attachmentRepository;
    private readonly IAuditService _audit;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ChangeManagementDbContext _context;

    public AttachmentService(
        IChangeAttachmentRepository attachmentRepository,
        IChangeRepository changeRepository,
        IAuditService audit,
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ChangeManagementDbContext context)
    {
        _attachmentRepository = attachmentRepository;
        _audit = audit;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _context = context;
    }

    public Task<List<ChangeAttachment>> GetForChangeAsync(Guid changeId, CancellationToken cancellationToken)
        => _attachmentRepository.GetByChangeIdAsync(changeId, cancellationToken);

    public Task<ChangeAttachment?> GetAsync(Guid attachmentId, CancellationToken cancellationToken)
        => _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

    public async Task<(ChangeAttachment? Attachment, string? Error)> UploadAsync(
        Guid changeId,
        IFormFile file,
        Guid? uploadedBy,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var changeInfo = await _context.ChangeRequests
            .Where(c => c.ChangeRequestId == changeId && c.DeletedAt == null)
            .Select(c => new
            {
                c.RequestedByUserId,
                c.CreatedBy,
                c.ChangeNumber
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (changeInfo is null)
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

        var rootPath = ResolveAttachmentRoot();
        Directory.CreateDirectory(rootPath);

        var fileId = await GenerateUniqueAttachmentIdAsync(cancellationToken);
        var safeName = Path.GetFileName(file.FileName);
        var storedFileName = $"{fileId}_{safeName}";

        var changeFolder = Path.Combine(rootPath, changeId.ToString());
        Directory.CreateDirectory(changeFolder);

        var storedPath = Path.GetFullPath(Path.Combine(changeFolder, storedFileName));
        long actualLength;

        try
        {
            await using var stream = new FileStream(storedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await file.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            actualLength = stream.Length;
        }
        catch
        {
            return (null, "Failed to persist attachment to disk.");
        }

        if (actualLength != file.Length)
        {
            TryDeletePhysicalFile(storedPath);
            return (null, "Attachment size mismatch after write.");
        }

        var resolvedUploader = uploadedBy;
        if (!resolvedUploader.HasValue || resolvedUploader == Guid.Empty)
        {
            resolvedUploader = changeInfo.RequestedByUserId != Guid.Empty
                ? changeInfo.RequestedByUserId
                : changeInfo.CreatedBy;
        }

        var entity = new ChangeAttachment
        {
            ChangeAttachmentId = fileId,
            ChangeRequestId = changeId,
            FileName = safeName,
            FilePath = storedPath,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = resolvedUploader,
            FileSizeBytes = actualLength
        };

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var created = await _attachmentRepository.CreateAsync(entity, cancellationToken);

            var actor = created.UploadedBy ?? changeInfo.CreatedBy;
            await _audit.LogAsync(
                5,
                actor,
                ResolveActorUpn(),
                "cm",
                "ChangeAttachment",
                created.ChangeAttachmentId,
                changeInfo.ChangeNumber.ToString(),
                "AttachmentUpload",
                created.FileName,
                cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return (created, null);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            TryDeletePhysicalFile(storedPath);
            return (null, "Failed to persist attachment metadata.");
        }
    }

    public async Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        var fullPath = attachment.FilePath;

        try
        {
            if (!string.IsNullOrWhiteSpace(fullPath) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            return false;
        }

        var deleted = await _attachmentRepository.DeleteAsync(attachmentId, cancellationToken);
        if (!deleted)
        {
            return false;
        }

        return true;
    }

    public async Task<byte[]?> ReadFileAsync(ChangeAttachment attachment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = attachment.FilePath;
        if (string.IsNullOrWhiteSpace(fullPath)) return null;

        var rootPath = ResolveAttachmentRoot();
        var normalizedRoot = Path.GetFullPath(rootPath + Path.DirectorySeparatorChar);
        var normalizedPath = Path.GetFullPath(fullPath);

        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)) return null;
        if (!File.Exists(normalizedPath)) return null;

        return await File.ReadAllBytesAsync(normalizedPath, cancellationToken);
    }

    private async Task<Guid> GenerateUniqueAttachmentIdAsync(CancellationToken cancellationToken)
    {
        Guid candidate;
        do
        {
            candidate = Guid.NewGuid();
        } while (await _context.ChangeAttachments.AnyAsync(x => x.ChangeAttachmentId == candidate, cancellationToken));

        return candidate;
    }

    private string ResolveAttachmentRoot()
    {
        var configured = NormalizePath(_configuration["AttachmentStorage:RootPath"]);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return EnsureAbsolutePath(configured);
        }

        var envPath = NormalizePath(Environment.GetEnvironmentVariable("ATTACHMENT_STORAGE_ROOT"));
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return EnsureAbsolutePath(envPath);
        }

        return EnsureAbsolutePath(Path.Combine(_environment.ContentRootPath, "data", "attachments"));
    }

    private string EnsureAbsolutePath(string path)
        => Path.GetFullPath(path);

    private static string NormalizePath(string? path)
    {
        var value = (path ?? string.Empty).Trim();
        return value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static void TryDeletePhysicalFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best effort cleanup
        }
    }

    private string ResolveActorUpn()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.Upn)
               ?? user?.FindFirstValue(ClaimTypes.Email)
               ?? user?.Identity?.Name
               ?? "unknown@local";
    }
}
