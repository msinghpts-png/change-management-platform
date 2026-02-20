namespace ChangeManagement.Api.Domain.Entities;

public class ChangeStatus
{
    public int StatusId { get; set; }
    public string? Name { get; set; }
    public bool IsTerminal { get; set; }

    public ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
}
