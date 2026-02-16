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

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=ChangeManagementDB;Trusted_Connection=true;TrustServerCertificate=true;";
builder.Services.AddDbContext<ChangeManagementDbContext>(options => options.UseSqlServer(connectionString));

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
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
    }

    var adminUpn = app.Configuration["SeedAdmin:Upn"] ?? "admin@local";
    var adminPassword = app.Configuration["SeedAdmin:Password"] ?? "Admin123!";
    if (!dbContext.Users.Any())
    {
        dbContext.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Upn = adminUpn,
            DisplayName = "Local Administrator",
            Role = "Admin",
            IsActive = true,
            PasswordHash = PasswordHasher.Hash(adminPassword)
        });
        dbContext.SaveChanges();
    }
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
