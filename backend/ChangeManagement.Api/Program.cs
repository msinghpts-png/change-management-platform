using ChangeManagement.Api.Data;
using ChangeManagement.Api.Repositories;
using ChangeManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=ChangeManagementDB;Trusted_Connection=true;TrustServerCertificate=true;";
builder.Services.AddDbContext<ChangeManagementDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddScoped<IChangeRepository, ChangeRepository>();
builder.Services.AddScoped<IChangeService, ChangeService>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IChangeAttachmentRepository, ChangeAttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IChangeTaskRepository, ChangeTaskRepository>();
builder.Services.AddScoped<IChangeTaskService, ChangeTaskService>();
builder.Services.AddScoped<IAuditService, AuditService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();
    var isSqlite = string.Equals(
        dbContext.Database.ProviderName,
        "Microsoft.EntityFrameworkCore.Sqlite",
        StringComparison.Ordinal);

    var discoveredMigrations = dbContext.Database.GetMigrations().ToList();
    if (discoveredMigrations.Count == 0)
    {
        throw new InvalidOperationException("No EF Core migrations were discovered. Ensure migration attributes and assembly scanning are configured correctly.");
    }

    if (isSqlite)
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        dbContext.Database.Migrate();
    }

    var missingTables = isSqlite
        ? dbContext.Database.SqlQueryRaw<string>(@"
SELECT required.TableName
FROM (VALUES
('ChangeRequest'),('ChangeTask'),('ChangeApproval'),('ChangeAttachment'),('[User]'),
('Event'),('EventType'),
('ChangeType'),('ChangePriority'),('ChangeStatus'),('RiskLevel'),('ApprovalStatus')
) AS required(TableName)
WHERE NOT EXISTS (
    SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = REPLACE(REPLACE(required.TableName, '[', ''), ']', '')
);").ToList()
        : dbContext.Database.SqlQueryRaw<string>(@"
SELECT required.TableName
FROM (VALUES
('cm.ChangeRequest'),('cm.ChangeTask'),('cm.ChangeApproval'),('cm.ChangeAttachment'),('cm.[User]'),
('audit.Event'),('audit.EventType'),
('ref.ChangeType'),('ref.ChangePriority'),('ref.ChangeStatus'),('ref.RiskLevel'),('ref.ApprovalStatus')
) AS required(TableName)
WHERE OBJECT_ID(required.TableName, 'U') IS NULL;").ToList();

    if (missingTables.Count > 0)
    {
        throw new InvalidOperationException($"Database validation failed. Missing required tables: {string.Join(", ", missingTables)}");
    }
}

app.MapControllers();
app.Run();

public partial class Program;
