using System.Security.Claims;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/attachments")]
public class AdminAttachmentsController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;
    private readonly IAuditService _audit;

    public AdminAttachmentsController(ChangeManagementDbContext dbContext, IAuditService audit)
    {
        _dbContext = dbContext;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? changeNumber, CancellationToken cancellationToken)
    {
        var query = _dbContext.ChangeAttachments
            .AsNoTracking()
            .Include(x => x.ChangeRequest)
            .Include(x => x.UploadedByUser)
            .OrderByDescending(x => x.UploadedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(changeNumber))
        {
            var normalized = changeNumber.Trim().ToUpperInvariant().Replace("CHG-", string.Empty);
            if (int.TryParse(normalized, out var num))
            {
                query = query.Where(x => x.ChangeRequest != null && x.ChangeRequest.ChangeNumber == num);
            }
        }

        var items = await query.Take(500).ToListAsync(cancellationToken);
        return Ok(items.Select(x => new
        {
            id = x.ChangeAttachmentId,
            changeRequestId = x.ChangeRequestId,
            changeNumber = x.ChangeRequest == null ? string.Empty : $"CHG-{x.ChangeRequest.ChangeNumber:D6}",
            fileName = x.FileName,
            filePath = ResolveAttachmentPath(x),
            sizeBytes = x.FileSizeBytes,
            uploadedAt = x.UploadedAt,
            uploadedBy = x.UploadedByUser?.Upn
        }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.ChangeAttachments
            .Include(x => x.ChangeRequest)
            .FirstOrDefaultAsync(x => x.ChangeAttachmentId == id, cancellationToken);
        if (attachment is null) return NotFound();

        if (System.IO.File.Exists(attachment.FilePath))
        {
            System.IO.File.Delete(attachment.FilePath);
        }

        _dbContext.ChangeAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var actorId = ResolveActorUserId();
        var actorUpn = ResolveActorUpn();
        await _audit.LogAsync(5, actorId, actorUpn, "cm", "ChangeAttachment", attachment.ChangeAttachmentId, attachment.ChangeRequest?.ChangeNumber.ToString() ?? string.Empty, "AttachmentDelete", attachment.FileName, cancellationToken);

        return NoContent();
    }


    private static string ResolveAttachmentPath(ChangeAttachment attachment)
    {
        var filePathProperty = attachment.GetType().GetProperty("FilePath");
        var filePathValue = filePathProperty?.GetValue(attachment) as string;
        return string.IsNullOrWhiteSpace(filePathValue) ? attachment.FilePath : filePathValue;
    }

    private Guid ResolveActorUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var parsed) && parsed != Guid.Empty ? parsed : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    private string ResolveActorUpn()
        => User.FindFirstValue(ClaimTypes.Upn)
           ?? User.FindFirstValue(ClaimTypes.Email)
           ?? User.Identity?.Name
           ?? "unknown@local";
}
