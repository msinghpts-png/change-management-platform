using System.Text;
using System.Text.Json;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs.Admin;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/admin/database")]
public class AdminController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;
    private readonly IDatabaseInitializer _databaseInitializer;

    public AdminController(ChangeManagementDbContext dbContext, IDatabaseInitializer databaseInitializer)
    {
        _dbContext = dbContext;
        _databaseInitializer = databaseInitializer;
    }

    [HttpGet("status")]
    public async Task<ActionResult<DatabaseStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var pending = (await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        var csb = new SqlConnectionStringBuilder(_dbContext.Database.GetConnectionString());

        return Ok(new DatabaseStatusDto
        {
            DatabaseName = csb.InitialCatalog,
            TotalChanges = await _dbContext.ChangeRequests.CountAsync(cancellationToken),
            TotalApprovals = await _dbContext.ChangeApprovals.CountAsync(cancellationToken),
            TotalAttachments = await _dbContext.ChangeAttachments.CountAsync(cancellationToken),
            HasPendingMigrations = pending.Count > 0,
            PendingMigrations = pending
        });
    }

    [HttpPost("migrate")]
    public async Task<IActionResult> RunMigrations(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        return Ok(new { message = "Migrations applied successfully." });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        var seeded = await _databaseInitializer.SeedIfEmptyAsync(cancellationToken);
        return Ok(new { seeded, message = seeded ? "Sample data seeded." : "Database already contains data. Skipped seeding." });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var backup = new DatabaseBackupDto
        {
            ChangeRequests = await _dbContext.ChangeRequests.AsNoTracking().ToListAsync(cancellationToken),
            ChangeApprovals = await _dbContext.ChangeApprovals.AsNoTracking().ToListAsync(cancellationToken),
            ChangeAttachments = await _dbContext.ChangeAttachments
                .AsNoTracking()
                .Select(attachment => new AttachmentBackupItemDto
                {
                    Id = attachment.Id,
                    ChangeRequestId = attachment.ChangeRequestId,
                    FileName = attachment.FileName,
                    ContentType = attachment.ContentType,
                    SizeBytes = attachment.SizeBytes,
                    StoragePath = attachment.StoragePath,
                    UploadedAt = attachment.UploadedAt,
                    ContentBase64 = System.IO.File.Exists(attachment.StoragePath)
                        ? Convert.ToBase64String(System.IO.File.ReadAllBytes(attachment.StoragePath))
                        : null
                })
                .ToListAsync(cancellationToken)
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
        return File(Encoding.UTF8.GetBytes(json), "application/json", $"change-management-backup-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] DatabaseBackupDto backup, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.ChangeApprovals.RemoveRange(_dbContext.ChangeApprovals);
        _dbContext.ChangeAttachments.RemoveRange(_dbContext.ChangeAttachments);
        _dbContext.ChangeRequests.RemoveRange(_dbContext.ChangeRequests);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var uploadRoot = "/app/uploads";
        Directory.CreateDirectory(uploadRoot);

        var attachments = new List<ChangeAttachment>();
        foreach (var item in backup.ChangeAttachments)
        {
            var extension = Path.GetExtension(item.FileName);
            var folder = Path.Combine(uploadRoot, item.ChangeRequestId.ToString("N"));
            Directory.CreateDirectory(folder);
            var storedPath = Path.Combine(folder, $"{item.Id:N}{extension}");

            if (!string.IsNullOrWhiteSpace(item.ContentBase64))
            {
                var bytes = Convert.FromBase64String(item.ContentBase64);
                await System.IO.File.WriteAllBytesAsync(storedPath, bytes, cancellationToken);
            }

            attachments.Add(new ChangeAttachment
            {
                Id = item.Id,
                ChangeRequestId = item.ChangeRequestId,
                FileName = item.FileName,
                ContentType = item.ContentType,
                SizeBytes = item.SizeBytes,
                StoragePath = storedPath,
                UploadedAt = item.UploadedAt
            });
        }

        _dbContext.ChangeRequests.AddRange(backup.ChangeRequests);
        _dbContext.ChangeApprovals.AddRange(backup.ChangeApprovals);
        _dbContext.ChangeAttachments.AddRange(attachments);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return Ok(new { message = "Database import completed successfully." });
    }
}
