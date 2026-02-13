using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface IDatabaseInitializer
{
    Task<DatabaseInitializationResult> InitializeAsync(CancellationToken cancellationToken = default);
    Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken = default);
}

public record DatabaseInitializationResult(bool DatabaseExisted, bool Seeded);

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ChangeManagementDbContext _dbContext;

    public DatabaseInitializer(ChangeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DatabaseInitializationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        var existed = await _dbContext.Database.CanConnectAsync(cancellationToken);
        await _dbContext.Database.MigrateAsync(cancellationToken);
        var seeded = await SeedIfEmptyAsync(cancellationToken);
        return new DatabaseInitializationResult(existed, seeded);
    }

    public async Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.ChangeRequests.AnyAsync(cancellationToken))
        {
            return false;
        }

        var firstChangeId = Guid.NewGuid();
        var secondChangeId = Guid.NewGuid();

        var changes = new[]
        {
            new ChangeRequest
            {
                Id = firstChangeId,
                Title = "Deploy Q1 Security Patch Rollup",
                Description = "Apply security patch rollup to production web servers.",
                Status = ChangeStatus.PendingApproval,
                Priority = "P2",
                RiskLevel = "Medium",
                PlannedStart = DateTime.UtcNow.AddDays(2),
                PlannedEnd = DateTime.UtcNow.AddDays(2).AddHours(2),
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                Id = secondChangeId,
                Title = "Emergency DB Performance Fix",
                Description = "Hotfix long-running query impacting checkout latency.",
                Status = ChangeStatus.Draft,
                Priority = "P1",
                RiskLevel = "High",
                PlannedStart = DateTime.UtcNow.AddDays(1),
                PlannedEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
                CreatedAt = DateTime.UtcNow
            }
        };

        var approvals = new[]
        {
            new ChangeApproval
            {
                Id = Guid.NewGuid(),
                ChangeRequestId = firstChangeId,
                Approver = "manager@example.com",
                Status = ApprovalStatus.Pending,
                Comment = "Awaiting CAB review"
            }
        };

        _dbContext.ChangeRequests.AddRange(changes);
        _dbContext.ChangeApprovals.AddRange(approvals);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
