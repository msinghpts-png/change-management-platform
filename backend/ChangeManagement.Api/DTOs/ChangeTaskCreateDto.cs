namespace ChangeManagement.Api.DTOs;

public class ChangeTaskCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
