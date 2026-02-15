namespace ChangeManagement.Api.Domain.Entities;

public class ChangePriority
{
    public int PriorityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
}
