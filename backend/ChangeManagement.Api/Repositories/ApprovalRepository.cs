using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;

namespace ChangeManagement.Api.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly ChangeManagementDbContext _dbContext;

    public ApprovalRepository(ChangeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ChangeApproval AddApproval(ChangeApproval approval)
    {
        _dbContext.ChangeApprovals.Add(approval);
        _dbContext.SaveChanges();
        return approval;
    }

    public IEnumerable<ChangeApproval> GetApprovalsForChange(Guid changeRequestId)
    {
        return _dbContext.ChangeApprovals.Where(approval => approval.ChangeRequestId == changeRequestId).ToList();
    }

    public ChangeApproval? GetById(Guid approvalId)
    {
        return _dbContext.ChangeApprovals.FirstOrDefault(item => item.Id == approvalId);
    }

    public ChangeApproval? UpdateDecision(Guid approvalId, ApprovalStatus status, string? comment, DateTime decidedAt)
    {
        var approval = GetById(approvalId);
        if (approval is null)
        {
            return null;
        }

        approval.Status = status;
        approval.Comment = comment;
        approval.DecisionAt = decidedAt;

        _dbContext.ChangeApprovals.Update(approval);
        _dbContext.SaveChanges();
        return approval;
    }
}
