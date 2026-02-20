namespace ChangeManagement.Api.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }

    public string Upn { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<ChangeRequest> RequestedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> AssignedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> ApprovalRequestedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> SubmittedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> DeletedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> CreatedChanges { get; set; } = new List<ChangeRequest>();
    public ICollection<ChangeRequest> UpdatedChanges { get; set; } = new List<ChangeRequest>();

    public ICollection<ChangeApprover> ChangeApproverSelections { get; set; } = new List<ChangeApprover>();
    public ICollection<ChangeAttachment> UploadedAttachments { get; set; } = new List<ChangeAttachment>();
    public ICollection<ChangeTask> AssignedTasks { get; set; } = new List<ChangeTask>();
    public ICollection<ChangeTemplate> CreatedTemplates { get; set; } = new List<ChangeTemplate>();

    public ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();
}
