using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Repositories;

public class ChangeRepository : IChangeRepository
{
    private readonly ChangeManagementDbContext _dbContext;

    public ChangeRepository(ChangeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<ChangeRequest> GetAll()
    {
        return _dbContext.ChangeRequests.Include(c => c.Approvals).ToList();
    }

    public ChangeRequest? GetById(Guid id)
    {
        return _dbContext.ChangeRequests.Include(c => c.Approvals).FirstOrDefault(change => change.Id == id);
    }

    public ChangeRequest Create(ChangeRequest changeRequest)
    {
        _dbContext.ChangeRequests.Add(changeRequest);
        _dbContext.SaveChanges();
        return changeRequest;
    }

    public ChangeRequest? Update(ChangeRequest changeRequest)
    {
        var existing = GetById(changeRequest.Id);
        if (existing is null)
        {
            return null;
        }

        existing.Title = changeRequest.Title;
        existing.Description = changeRequest.Description;
        existing.Status = changeRequest.Status;
        existing.Priority = changeRequest.Priority;
        existing.RiskLevel = changeRequest.RiskLevel;
        existing.PlannedStart = changeRequest.PlannedStart;
        existing.PlannedEnd = changeRequest.PlannedEnd;
        existing.UpdatedAt = changeRequest.UpdatedAt;

        _dbContext.ChangeRequests.Update(existing);
        _dbContext.SaveChanges();
        return existing;
    }
}
