using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.DTOs;

public class AttachmentUploadDto
{
    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }
}
