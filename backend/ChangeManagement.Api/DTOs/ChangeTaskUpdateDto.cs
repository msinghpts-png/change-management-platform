namespace ChangeManagement.Api.DTOs;

public class ChangeTaskUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
