using ChangeManagement.Api.Domain.Entities;

namespace ChangeManagement.Api.Repositories;

public interface IChangeRepository
{
    IEnumerable<ChangeRequest> GetAll();
    ChangeRequest? GetById(Guid id);
    ChangeRequest Create(ChangeRequest changeRequest);
    ChangeRequest? Update(ChangeRequest changeRequest);
}
