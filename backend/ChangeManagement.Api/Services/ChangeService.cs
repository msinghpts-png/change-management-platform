using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;

namespace ChangeManagement.Api.Services;

public interface IChangeService
{
    IEnumerable<ChangeRequest> GetAll();
    ChangeRequest? GetById(Guid id);
    ChangeRequest Create(ChangeRequest changeRequest);
    ChangeRequest? Update(ChangeRequest changeRequest);
}

public class ChangeService : IChangeService
{
    private readonly IChangeRepository _repository;

    public ChangeService(IChangeRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<ChangeRequest> GetAll()
    {
        return _repository.GetAll();
    }

    public ChangeRequest? GetById(Guid id)
    {
        return _repository.GetById(id);
    }

    public ChangeRequest Create(ChangeRequest changeRequest)
    {
        return _repository.Create(changeRequest);
    }

    public ChangeRequest? Update(ChangeRequest changeRequest)
    {
        return _repository.Update(changeRequest);
    }
}
