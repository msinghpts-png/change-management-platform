namespace ChangeManagement.Api.Domain.Entities;

public class RiskLevel
{
    public int RiskLevelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }

    public ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
}
