using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/changes/{changeId:guid}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(Guid changeId, CancellationToken cancellationToken)
    {
        var items = await _attachmentService.GetForChangeAsync(changeId, cancellationToken);
        return Ok(items.Select(i => new
        {
            i.ChangeAttachmentId,
            i.ChangeId,
            i.FileName,
            i.ContentType,
            i.FileSizeBytes,
            i.UploadedAt
        }));
    }
}
