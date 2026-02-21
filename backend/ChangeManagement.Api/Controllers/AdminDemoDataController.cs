using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/demo-data")]
public class AdminDemoDataController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public AdminDemoDataController(ChangeManagementDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [HttpPost]
    public async Task<IActionResult> Load(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return BadRequest(new { message = "Demo data can only be loaded in Development." });
        }

        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cab1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var cab2 = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var user1 = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var addedUsers = 0;
        var addedTemplates = 0;
        var addedChanges = 0;

        if (!await _dbContext.Users.AnyAsync(x => x.UserId == adminId, cancellationToken))
        {
            _dbContext.Users.AddRange(
                new User { UserId = adminId, Upn = "admin@local", DisplayName = "Admin User", Role = "Admin", IsActive = true, PasswordHash = PasswordHasher.Hash("Admin123!") },
                new User { UserId = cab1, Upn = "cab1@local", DisplayName = "CAB One", Role = "CAB", IsActive = true, PasswordHash = PasswordHasher.Hash("Admin123!") },
                new User { UserId = cab2, Upn = "cab2@local", DisplayName = "CAB Two", Role = "CAB", IsActive = true, PasswordHash = PasswordHasher.Hash("Admin123!") },
                new User { UserId = user1, Upn = "user1@local", DisplayName = "Standard User", Role = "User", IsActive = true, PasswordHash = PasswordHasher.Hash("Admin123!") }
            );
            addedUsers += 4;
        }

        if (!await _dbContext.ChangeTemplates.AnyAsync(cancellationToken))
        {
            _dbContext.ChangeTemplates.AddRange(
                new ChangeTemplate { TemplateId = Guid.NewGuid(), Name = "Windows Security Patch", Description = "Monthly patching", Category = "Server", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = adminId },
                new ChangeTemplate { TemplateId = Guid.NewGuid(), Name = "Firewall Rule Change", Description = "Firewall update", Category = "Network", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = adminId },
                new ChangeTemplate { TemplateId = Guid.NewGuid(), Name = "DB Maintenance", Description = "Database maintenance", Category = "Database", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = adminId }
            );
            addedTemplates += 3;
        }

        if (!await _dbContext.ChangeRequests.AnyAsync(cancellationToken))
        {
            var draft = new ChangeRequest
            {
                ChangeRequestId = Guid.NewGuid(),
                Title = "Demo Draft Change",
                Description = "Draft",
                ChangeTypeId = 1,
                PriorityId = 2,
                StatusId = 1,
                RiskLevelId = 2,
                ImpactTypeId = 2,
                RequestedByUserId = user1,
                CreatedBy = user1,
                CreatedAt = DateTime.UtcNow
            };
            var submitted = new ChangeRequest
            {
                ChangeRequestId = Guid.NewGuid(),
                Title = "Demo Submitted Change",
                Description = "Submitted",
                ChangeTypeId = 2,
                PriorityId = 2,
                StatusId = 2,
                RiskLevelId = 2,
                ImpactTypeId = 2,
                RequestedByUserId = user1,
                CreatedBy = user1,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.ChangeRequests.AddRange(draft, submitted);
            _dbContext.ChangeApprovers.Add(new ChangeApprover { ChangeApproverId = Guid.NewGuid(), ChangeRequestId = submitted.ChangeRequestId, ApproverUserId = cab1, ApprovalStatus = "Pending", CreatedAt = DateTime.UtcNow });
            _dbContext.ChangeTasks.Add(new ChangeTask { ChangeTaskId = Guid.NewGuid(), ChangeRequestId = submitted.ChangeRequestId, Title = "Review implementation plan", Description = "CAB checklist", Status = "Open", CreatedAt = DateTime.UtcNow });
            _dbContext.ChangeAttachments.Add(new ChangeAttachment { ChangeAttachmentId = Guid.NewGuid(), ChangeRequestId = draft.ChangeRequestId, FileName = "demo.txt", FilePath = string.Empty, UploadedAt = DateTime.UtcNow, UploadedBy = adminId, FileSizeBytes = 0 });
            addedChanges += 2;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Demo data loaded.", usersAdded = addedUsers, templatesAdded = addedTemplates, changesAdded = addedChanges });
    }
}
