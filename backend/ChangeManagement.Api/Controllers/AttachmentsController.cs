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

    public AttachmentsController(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> List(Guid changeId, CancellationToken cancellationToken)
    {
        var items = await _attachmentService.GetForChangeAsync(changeId, cancellationToken);
        return Ok(items.Select(i => i.ToDto()));
    }

    [HttpPost]
    public async Task<ActionResult<AttachmentDto>> Upload(Guid changeId, [FromForm] AttachmentUploadDto request, [FromQuery] Guid uploadedBy, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.UploadAsync(changeId, request.File, uploadedBy, cancellationToken);
        return result.Attachment is null ? BadRequest(result.Error) : CreatedAtAction(nameof(List), new { changeId }, result.Attachment.ToDto());
    }

    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid changeId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = await _attachmentService.GetAsync(attachmentId, cancellationToken);
        if (attachment is null || attachment.ChangeRequestId != changeId) return NotFound();

        var bytes = await _attachmentService.ReadFileAsync(attachment, cancellationToken);
        return bytes is null ? NotFound() : File(bytes, "application/octet-stream", attachment.FileName);
    }

    [HttpDelete("{attachmentId:guid}")]
    public async Task<IActionResult> Delete(Guid attachmentId, CancellationToken cancellationToken)
        => await _attachmentService.DeleteAsync(attachmentId, cancellationToken) ? NoContent() : NotFound();
}
