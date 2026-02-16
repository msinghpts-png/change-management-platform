using System.Security.Claims;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IApprovalService
{
    Task<ChangeApproval> CreateApprovalAsync(ChangeApproval approval, CancellationToken cancellationToken);
    Task<List<ChangeApproval>> GetApprovalsForChangeAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<ChangeApproval?> RecordDecisionAsync(Guid changeId, Guid approvalId, int approvalStatusId, string comments, CancellationToken cancellationToken);
    Task<ChangeApproval?> ApproveChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken);
    Task<ChangeApproval?> RejectChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repository;
    private readonly IChangeRepository _changeRepository;
    private readonly IChangeService _changeService;
    private readonly IAuditService _audit;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApprovalService(IApprovalRepository repository, IChangeRepository changeRepository, IChangeService changeService, IAuditService audit, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _changeRepository = changeRepository;
        _changeService = changeService;
        _audit = audit;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<ChangeApproval> CreateApprovalAsync(ChangeApproval approval, CancellationToken cancellationToken) => _repository.CreateAsync(approval, cancellationToken);
    public Task<List<ChangeApproval>> GetApprovalsForChangeAsync(Guid changeRequestId, CancellationToken cancellationToken) => _repository.GetByChangeAsync(changeRequestId, cancellationToken);

    public async Task<ChangeApproval?> RecordDecisionAsync(Guid changeId, Guid approvalId, int approvalStatusId, string comments, CancellationToken cancellationToken)
    {
        var approval = await _repository.GetByIdAsync(approvalId, cancellationToken);
        if (approval is null || approval.ChangeRequestId != changeId)
        {
            return null;
        }

        approval.ApprovalStatusId = approvalStatusId;
        approval.Comments = comments;
        approval.ApprovedAt = DateTime.UtcNow;
        await _repository.SaveAsync(cancellationToken);

        var change = await _changeRepository.GetByIdAsync(changeId, cancellationToken);
        if (change is not null)
        {
            if (approvalStatusId == 2) change.StatusId = 3;
            if (approvalStatusId == 3) change.StatusId = 4;
            await _changeService.UpdateAsync(change, cancellationToken);
            await _audit.LogAsync(4, approval.ApproverUserId, ResolveActorUpn(), "cm", "ChangeApproval", approval.ChangeApprovalId, change.ChangeNumber.ToString(), approvalStatusId == 2 ? "Approve" : "Reject", comments, cancellationToken);
        }

        return approval;
    }

    public Task<ChangeApproval?> ApproveChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken)
        => CreateOrDecideAsync(changeId, cabUserId, 2, comments, cancellationToken);

    public Task<ChangeApproval?> RejectChangeAsync(Guid changeId, Guid cabUserId, string comments, CancellationToken cancellationToken)
        => CreateOrDecideAsync(changeId, cabUserId, 3, comments, cancellationToken);

    private string ResolveActorUpn()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.Email)
               ?? user?.Identity?.Name
               ?? "system@local";
    }

    private async Task<ChangeApproval?> CreateOrDecideAsync(Guid changeId, Guid cabUserId, int approvalStatusId, string comments, CancellationToken cancellationToken)
    {
        var approvals = await _repository.GetByChangeAsync(changeId, cancellationToken);
        var approval = approvals.FirstOrDefault(a => a.ApproverUserId == cabUserId)
            ?? await _repository.CreateAsync(new ChangeApproval
            {
                ChangeApprovalId = Guid.NewGuid(),
                ChangeRequestId = changeId,
                ApproverUserId = cabUserId,
                ApprovalStatusId = 1,
                Comments = string.Empty
            }, cancellationToken);

        return await RecordDecisionAsync(changeId, approval.ChangeApprovalId, approvalStatusId, comments, cancellationToken);
    }
}
