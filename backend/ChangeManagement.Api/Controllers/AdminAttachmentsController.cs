using ChangeManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/attachments")]
public class AdminAttachmentsController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public AdminAttachmentsController(ChangeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? changeNumber, CancellationToken cancellationToken)
    {
        var query = _dbContext.ChangeAttachments
            .AsNoTracking()
            .Include(x => x.ChangeRequest)
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
            filePath = x.FileUrl,
            sizeBytes = x.FileSizeBytes,
            uploadedAt = x.UploadedAt
        }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.ChangeAttachments.FirstOrDefaultAsync(x => x.ChangeAttachmentId == id, cancellationToken);
        if (attachment is null) return NotFound();

        if (System.IO.File.Exists(attachment.FileUrl))
        {
            System.IO.File.Delete(attachment.FileUrl);
        }

        _dbContext.ChangeAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
