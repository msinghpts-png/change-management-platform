namespace ChangeManagement.Api.Domain.Entities;

public class ChangeTask
{
    public Guid ChangeTaskId { get; set; }
    public Guid ChangeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public User? AssignedToUser { get; set; }
}
