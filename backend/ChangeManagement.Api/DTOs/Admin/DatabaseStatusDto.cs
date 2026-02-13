namespace ChangeManagement.Api.DTOs.Admin;

public class DatabaseStatusDto
{
    public string DatabaseName { get; set; } = string.Empty;
    public int TotalChanges { get; set; }
    public int TotalApprovals { get; set; }
    public int TotalAttachments { get; set; }
    public bool HasPendingMigrations { get; set; }
    public IReadOnlyCollection<string> PendingMigrations { get; set; } = [];
}
