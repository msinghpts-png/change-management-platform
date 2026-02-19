using System.Text.Json;

namespace ChangeManagement.Api.DTOs;

public class TransitionAuditDto
{
    public string Details { get; set; } = string.Empty;
    public JsonElement? TransitionDetails { get; set; }
}
