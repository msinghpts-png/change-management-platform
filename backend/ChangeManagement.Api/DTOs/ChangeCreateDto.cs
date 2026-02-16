using System.ComponentModel.DataAnnotations;
using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.DTOs;

public class ChangeCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public ChangeType? ChangeType { get; set; }
    [Required]
    public RiskLevel? RiskLevel { get; set; }
    [Required]
    public DateTime? ImplementationDate { get; set; }
    [Required]
    public string ImpactDescription { get; set; } = string.Empty;
    [Required]
    public string RollbackPlan { get; set; } = string.Empty;
    public Guid? AssignedToUserId { get; set; }
}
