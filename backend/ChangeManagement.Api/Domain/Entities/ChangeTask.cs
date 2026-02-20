namespace ChangeManagement.Api.Domain.Entities;

public class ChangeTask
{
    public Guid ChangeTaskId { get; set; }
    public Guid ChangeRequestId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? AssignedToUser { get; set; }
}
