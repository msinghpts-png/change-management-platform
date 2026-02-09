using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public class ChangeRepository : IChangeRepository
{
    private readonly List<ChangeRequest> _changes = new();

    public IEnumerable<ChangeRequest> GetAll()
    {
        return _changes;
    }

    public ChangeRequest? GetById(Guid id)
    {
        return _changes.FirstOrDefault(change => change.Id == id);
    }

    public ChangeRequest Create(ChangeRequest changeRequest)
    {
        _changes.Add(changeRequest);
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

        return existing;
    }
}
