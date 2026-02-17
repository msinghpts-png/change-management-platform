using System.Text;
using System.Text.Json;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Repositories;
using ChangeManagement.Api.Security;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=ChangeManagementDB;Trusted_Connection=true;TrustServerCertificate=true;";
var useInMemoryDatabase = builder.Environment.IsEnvironment("Testing") || builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

builder.Services.AddDbContext<ChangeManagementDbContext>(options =>
{
    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("ChangeManagement.Testing");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Change Management API",
        Version = "v1"
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "local-dev-super-secret-key-change-me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChangeManagement.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ChangeManagement.Frontend";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IChangeRepository, ChangeRepository>();
builder.Services.AddScoped<IChangeService, ChangeService>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IChangeAttachmentRepository, ChangeAttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IChangeTaskRepository, ChangeTaskRepository>();
builder.Services.AddScoped<IChangeTaskService, ChangeTaskService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var errorMessage = feature?.Error?.Message ?? "An unexpected error occurred.";

        var payload = JsonSerializer.Serialize(new
        {
            message = errorMessage,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    });
});

var skipDatabaseInitialization = app.Environment.IsEnvironment("Testing") || app.Configuration.GetValue<bool>("SkipDatabaseInitialization");
if (!skipDatabaseInitialization)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();
    var isSqlite = string.Equals(dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal);

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
        dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('cm.ChangeAttachment', 'FileSizeBytes') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeAttachment]
    ADD [FileSizeBytes] BIGINT NOT NULL CONSTRAINT [DF_ChangeAttachment_FileSizeBytes] DEFAULT(0);
END
");
        dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('cm.ChangeRequest', 'ImpactTypeId') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeRequest]
    ADD [ImpactTypeId] INT NULL;
END
IF OBJECT_ID('cm.ChangeTemplate', 'U') IS NULL
BEGIN
    CREATE TABLE [cm].[ChangeTemplate](
        [TemplateId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [ImplementationSteps] NVARCHAR(MAX) NULL,
        [BackoutPlan] NVARCHAR(MAX) NULL,
        [ServiceSystem] NVARCHAR(200) NULL,
        [Category] NVARCHAR(200) NULL,
        [Environment] NVARCHAR(200) NULL,
        [BusinessJustification] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT(1)
    );
END

IF COL_LENGTH('cm.ChangeTemplate', 'ChangeTypeId') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeTemplate] ADD [ChangeTypeId] INT NULL;
END
IF COL_LENGTH('cm.ChangeTemplate', 'RiskLevelId') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeTemplate] ADD [RiskLevelId] INT NULL;
END
IF NOT EXISTS (SELECT 1 FROM [audit].[EventType] WHERE [EventTypeId] = 6)
BEGIN
    INSERT INTO [audit].[EventType]([EventTypeId],[Name],[Description]) VALUES (6,'TemplateCreated','Template created');
END
IF NOT EXISTS (SELECT 1 FROM [audit].[EventType] WHERE [EventTypeId] = 7)
BEGIN
    INSERT INTO [audit].[EventType]([EventTypeId],[Name],[Description]) VALUES (7,'TemplateUpdated','Template updated');
END
");
    }

    var adminUpn = app.Configuration["SeedAdmin:Upn"] ?? "admin@local";
    var adminPassword = app.Configuration["SeedAdmin:Password"] ?? "Admin123!";
    var adminUser = dbContext.Users.FirstOrDefault(x => x.Upn == adminUpn);
    if (adminUser is null)
    {
        dbContext.Users.Add(new User
        {
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Upn = adminUpn,
            DisplayName = "Local Administrator",
            Role = "Admin",
            IsActive = true,
            PasswordHash = PasswordHasher.Hash(adminPassword)
        });
    }
    else
    {
        adminUser.Role = "Admin";
        adminUser.IsActive = true;
        if (!PasswordHasher.Verify(adminPassword, adminUser.PasswordHash))
        {
            adminUser.PasswordHash = PasswordHasher.Hash(adminPassword);
        }
    }

    if (!dbContext.Users.Any(x => x.Role == "CAB"))
    {
        dbContext.Users.Add(new User
        {
            UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Upn = "cab@local",
            DisplayName = "CAB User",
            Role = "CAB",
            IsActive = true,
            PasswordHash = PasswordHasher.Hash("Admin123!")
        });
    }

    dbContext.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
