using System.ComponentModel.DataAnnotations;

namespace ChangeManagement.Api.DTOs;

public class ChangeTaskCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
