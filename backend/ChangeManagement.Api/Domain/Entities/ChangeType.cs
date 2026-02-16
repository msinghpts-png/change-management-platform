namespace ChangeManagement.Api.Domain.Entities;

public class ChangeType
{
    public int ChangeTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
}
