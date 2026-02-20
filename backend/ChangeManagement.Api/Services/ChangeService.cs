using System.Security.Claims;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;
using Microsoft.AspNetCore.Http;

namespace ChangeManagement.Api.Services;

public interface IChangeService
{
    Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken);
    Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken);
}

public class ChangeService : IChangeService
{
    private readonly IChangeRepository _repository;
    private readonly IAuditService _audit;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IActorResolver _actorResolver;

    public ChangeService(IChangeRepository repository, IAuditService audit, IHttpContextAccessor httpContextAccessor, IActorResolver actorResolver)
    {
        _repository = repository;
        _audit = audit;
        _httpContextAccessor = httpContextAccessor;
        _actorResolver = actorResolver;
    }

    public Task<List<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken) => _repository.GetAllAsync(cancellationToken);
    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<ChangeRequest> CreateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var actorUserId = ResolveActorUserId();
        changeRequest.CreatedAt = DateTime.UtcNow;
        changeRequest.CreatedBy = actorUserId;

        var created = await _repository.CreateAsync(changeRequest, cancellationToken);
        await _audit.LogAsync(1, created.CreatedBy, ResolveActorUpn(), "cm", "ChangeRequest", created.ChangeRequestId, created.ChangeNumber.ToString(), "Create", created.Title, cancellationToken);
        return created;
    }

    public async Task<ChangeRequest?> UpdateAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(changeRequest.ChangeRequestId, cancellationToken);
        if (existing is null) return null;

        if (existing.StatusId >= 3)
        {
            return null;
        }

        ChangeRequest payload = changeRequest;
        if (existing.StatusId == 2)
        {
            existing.AssignedToUserId = changeRequest.AssignedToUserId;
            existing.PlannedStart = changeRequest.PlannedStart;
            existing.PlannedEnd = changeRequest.PlannedEnd;
            existing.BusinessJustification = changeRequest.BusinessJustification;
            existing.ImpactTypeId = changeRequest.ImpactTypeId;
            existing.UpdatedBy = changeRequest.UpdatedBy;
            payload = existing;
        }

        payload.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(payload, cancellationToken);
        if (updated is not null)
        {
            var reason = existing.StatusId == 2 ? "SubmittedEdit" : "Update";
            await _audit.LogAsync(2, updated.UpdatedBy ?? updated.CreatedBy, ResolveActorUpn(), "cm", "ChangeRequest", updated.ChangeRequestId, updated.ChangeNumber.ToString(), reason, updated.Title, cancellationToken);
        }

        return updated;
    }

    private string ResolveActorUpn()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.Upn)
               ?? user?.FindFirstValue(ClaimTypes.Email)
               ?? user?.Identity?.Name
               ?? "unknown@local";
    }

    private Guid ResolveActorUserId() => _actorResolver.ResolveActorUserId();
}
