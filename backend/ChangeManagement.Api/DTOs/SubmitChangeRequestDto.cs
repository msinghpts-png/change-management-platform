using System.ComponentModel.DataAnnotations;
using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.DTOs;

public class SubmitChangeRequestDto : IValidatableObject
{
    public List<Guid> ApproverUserIds { get; set; } = new();
    public string? ApprovalStrategy { get; set; }
    public string? Reason { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ApprovalStrategy))
        {
            yield break;
        }

        var isValid = string.Equals(ApprovalStrategy, ApprovalStrategies.Any, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ApprovalStrategy, ApprovalStrategies.Majority, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ApprovalStrategy, ApprovalStrategies.All, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            yield return new ValidationResult(
                "ApprovalStrategy must be one of: Any, Majority, All.",
                new[] { nameof(ApprovalStrategy) });
        }
    }
}
