using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.DTOs;

public class AttachmentUploadDto
{
    public IFormFile? File { get; set; }

    [FromForm(Name = "file")]
    public IFormFile? FileLowerCase { get; set; }
}
