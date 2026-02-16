using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IChangeService
{
    Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(ChangeRequest? Change, string? Error)> CreateAsync(ChangeCreateDto request, Guid createdByUserId, CancellationToken cancellationToken);
    Task<(ChangeRequest? Change, string? Error)> UpdateAsync(Guid changeId, ChangeUpdateDto request, Guid actorUserId, CancellationToken cancellationToken);
    Task<(ChangeRequest? Change, string? Error)> TransitionAsync(Guid changeId, ChangeStatus targetStatus, Guid actorUserId, CancellationToken cancellationToken);
}

public class ChangeService : IChangeService
{
    private readonly IChangeRepository _repository;
    private readonly IAuditService _audit;

    public ChangeService(IChangeRepository repository, IAuditService audit)
    {
        _repository = repository;
        _audit = audit;
    }

    public Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken) => _repository.GetAllAsync(cancellationToken);
    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<(ChangeRequest? Change, string? Error)> CreateAsync(ChangeCreateDto request, Guid createdByUserId, CancellationToken cancellationToken)
    {
        if (request.ChangeType is null || request.RiskLevel is null || request.ImplementationDate is null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ImpactDescription) || string.IsNullOrWhiteSpace(request.RollbackPlan))
        {
            return (null, "Missing required fields.");
        }

        var entity = new ChangeRequest
        {
            ChangeId = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ChangeType = request.ChangeType.Value,
            RiskLevel = request.RiskLevel.Value,
            ImpactDescription = request.ImpactDescription.Trim(),
            RollbackPlan = request.RollbackPlan.Trim(),
            ImplementationDate = request.ImplementationDate,
            Status = ChangeStatus.Draft,
            CreatedByUserId = createdByUserId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        await _audit.LogAsync(createdByUserId, "ChangeCreated", $"Change {created.ChangeNumber} created.", created.ChangeId, cancellationToken);
        return (created, null);
    }

    public async Task<(ChangeRequest? Change, string? Error)> UpdateAsync(Guid changeId, ChangeUpdateDto request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(changeId, cancellationToken);
        if (existing is null) return (null, "Change not found.");
        if (!ChangeWorkflowGuard.CanEdit(existing)) return (null, "Change cannot be edited after approval.");

        existing.Title = string.IsNullOrWhiteSpace(request.Title) ? existing.Title : request.Title.Trim();
        existing.Description = request.Description?.Trim() ?? existing.Description;
        existing.ChangeType = request.ChangeType ?? existing.ChangeType;
        existing.RiskLevel = request.RiskLevel ?? existing.RiskLevel;
        existing.ImpactDescription = string.IsNullOrWhiteSpace(request.ImpactDescription) ? existing.ImpactDescription : request.ImpactDescription.Trim();
        existing.RollbackPlan = string.IsNullOrWhiteSpace(request.RollbackPlan) ? existing.RollbackPlan : request.RollbackPlan.Trim();
        existing.ImplementationDate = request.ImplementationDate ?? existing.ImplementationDate;
        existing.AssignedToUserId = request.AssignedToUserId;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        await _audit.LogAsync(actorUserId, "ChangeUpdated", $"Change {existing.ChangeNumber} updated.", existing.ChangeId, cancellationToken);
        return (updated, null);
    }

    public async Task<(ChangeRequest? Change, string? Error)> TransitionAsync(Guid changeId, ChangeStatus targetStatus, Guid actorUserId, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(changeId, cancellationToken);
        if (existing is null) return (null, "Change not found.");

        if (!ChangeWorkflowGuard.CanTransition(existing.Status, targetStatus))
        {
            return (null, $"Invalid workflow transition from {existing.Status} to {targetStatus}.");
        }

        existing.Status = targetStatus;
        existing.UpdatedAt = DateTime.UtcNow;

        if (targetStatus == ChangeStatus.Approved) existing.ApprovedDate = DateTime.UtcNow;
        if (targetStatus == ChangeStatus.InImplementation) existing.ImplementationStartDate = DateTime.UtcNow;
        if (targetStatus == ChangeStatus.Completed) existing.CompletedDate = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        await _audit.LogAsync(actorUserId, "StatusChanged", $"Status changed from {existing.Status} to {targetStatus}.", existing.ChangeId, cancellationToken);
        return (updated, null);
    }
}
