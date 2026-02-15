using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ChangeManagementDbContext _dbContext;

    public DatabaseInitializer(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        if (!await _dbContext.Users.AnyAsync(cancellationToken))
        {
            _dbContext.Users.Add(new User
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Upn = "system@local",
                DisplayName = "System User",
                Role = "Admin",
                IsActive = true
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
