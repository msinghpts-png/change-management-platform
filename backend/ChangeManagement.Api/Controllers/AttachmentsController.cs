using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Extensions;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/changes/{changeId:guid}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<AttachmentDto>> List(Guid changeId)
    {
        var items = _attachmentService.GetForChange(changeId).Select(item => item.ToDto());
        return Ok(items);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<AttachmentDto>> Upload(Guid changeId, [FromForm] AttachmentUploadDto request, CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest("File is required.");
        }

        var result = await _attachmentService.UploadAsync(changeId, request.File, cancellationToken);
        if (result.Attachment is null)
        {
            if (string.Equals(result.Error, "Change request not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(List), new { changeId }, result.Attachment.ToDto());
    }

    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid changeId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = _attachmentService.Get(attachmentId);
        if (attachment is null || attachment.ChangeRequestId != changeId)
        {
            return NotFound();
        }

        var bytes = await _attachmentService.ReadFileAsync(attachment, cancellationToken);
        if (bytes is null)
        {
            return NotFound();
        }

        return File(bytes, attachment.ContentType, attachment.FileName);
    }

    [HttpDelete("{attachmentId:guid}")]
    public async Task<IActionResult> Delete(Guid changeId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = _attachmentService.Get(attachmentId);
        if (attachment is null || attachment.ChangeRequestId != changeId)
        {
            return NotFound();
        }

        await _attachmentService.DeleteAsync(attachmentId, cancellationToken);
        return NoContent();
    }
}
