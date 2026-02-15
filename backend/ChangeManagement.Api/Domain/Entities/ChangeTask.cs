namespace ChangeManagement.Api.Domain.Entities;

public class ChangeTask
{
    public Guid ChangeTaskId { get; set; }
    public Guid ChangeRequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ChangeRequest? ChangeRequest { get; set; }
    public ChangeStatus? Status { get; set; }
    public User? AssignedToUser { get; set; }
}
