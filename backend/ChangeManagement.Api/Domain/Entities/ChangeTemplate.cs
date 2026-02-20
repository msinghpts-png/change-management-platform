using System;

namespace ChangeManagement.Api.Domain.Entities;

public class ChangeTemplate
{
    public Guid TemplateId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public string? ImplementationSteps { get; set; }
    public string? BackoutPlan { get; set; }
    public string? BusinessJustification { get; set; }

    public string? ServiceSystem { get; set; }
    public string? Category { get; set; }
    public string? Environment { get; set; }

    public int? ChangeTypeId { get; set; }
    public int? RiskLevelId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    // ✅ RESTORE NAVIGATION
    public User CreatedByUser { get; set; } = default!;
}
