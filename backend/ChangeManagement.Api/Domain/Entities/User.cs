namespace ChangeManagement.Api.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Upn { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<ChangeRequest> CreatedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> AssignedChanges { get; set; } = new List<ChangeRequest>();
}
