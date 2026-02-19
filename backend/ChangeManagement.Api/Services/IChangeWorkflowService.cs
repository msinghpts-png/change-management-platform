using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Services;

public interface IChangeWorkflowService
{
    Task<ChangeRequest?> SubmitAsync(Guid changeId, Guid actorUserId, IReadOnlyCollection<Guid> approverUserIds, string? approvalStrategy, string? reason, CancellationToken cancellationToken);
    Task<ChangeRequest?> ApproveAsync(Guid changeId, Guid actorUserId, string? comments, CancellationToken cancellationToken);
    Task<ChangeRequest?> RejectAsync(Guid changeId, Guid actorUserId, string? comments, CancellationToken cancellationToken);
    Task<ChangeRequest?> RevertToDraftAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken);
    Task<ChangeRequest?> StartAsync(Guid changeId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ChangeRequest?> CompleteAsync(Guid changeId, Guid actorUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ChangeRequest?> CloseAsync(Guid changeId, Guid actorUserId, CancellationToken cancellationToken);
    Task<ChangeRequest?> CancelAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken);
    Task<ChangeRequest?> SoftDeleteAsync(Guid changeId, Guid actorUserId, string? reason, CancellationToken cancellationToken);
}
