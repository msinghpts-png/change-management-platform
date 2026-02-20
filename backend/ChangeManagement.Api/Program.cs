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

var useInMemoryDatabase =
    builder.Environment.IsEnvironment("Testing") ||
    builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

builder.Services.AddDbContext<ChangeManagementDbContext>(options =>
{
    if (useInMemoryDatabase)
        options.UseInMemoryDatabase("ChangeManagement.Testing");
    else
        options.UseSqlServer(connectionString);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

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
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var roles = identity.FindAll("role").Select(c => c.Value).ToList();

                    foreach (var role in roles)
                    {
                        if (!identity.HasClaim(ClaimTypes.Role, role))
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));

                        if (!identity.HasClaim("role", role))
                            identity.AddClaim(new Claim("role", role));
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IChangeRepository, ChangeRepository>();
builder.Services.AddScoped<IChangeService, ChangeService>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IChangeWorkflowService, ChangeWorkflowService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IChangeAttachmentRepository, ChangeAttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IChangeTaskRepository, ChangeTaskRepository>();
builder.Services.AddScoped<IChangeTaskService, ChangeTaskService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IActorResolver, ActorResolver>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var statusCode =
            feature?.Error is UnauthorizedAccessException
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            message = feature?.Error?.Message ?? "An unexpected error occurred.",
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    });
});

var skipDatabaseInitialization =
    app.Environment.IsEnvironment("Testing") ||
    app.Configuration.GetValue<bool>("SkipDatabaseInitialization");

if (!skipDatabaseInitialization)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();

    var provider = dbContext.Database.ProviderName ?? string.Empty;
    var isInMemory = provider == "Microsoft.EntityFrameworkCore.InMemory";
    var isSqlite = provider == "Microsoft.EntityFrameworkCore.Sqlite";

    if (isInMemory || isSqlite)
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        var migrations = dbContext.Database.GetMigrations().ToList();
        if (!migrations.Any())
            throw new InvalidOperationException("No EF Core migrations discovered.");

        dbContext.Database.Migrate();


    }

    SeedUsers(dbContext, app.Configuration);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void SeedUsers(ChangeManagementDbContext db, IConfiguration config)
{
    var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var cabId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    var adminPassword = config["SeedAdmin:Password"] ?? "Admin123!";

    if (!db.Users.Any(x => x.UserId == adminId))
    {
        db.Users.Add(new User
        {
            UserId = adminId,
            Upn = "admin@local",
            DisplayName = "Local Administrator",
            Role = "Admin",
            IsActive = true,
            PasswordHash = PasswordHasher.Hash(adminPassword)
        });
    }

    if (!db.Users.Any(x => x.UserId == cabId))
    {
        db.Users.Add(new User
        {
            UserId = cabId,
            Upn = "cab@local",
            DisplayName = "CAB User",
            Role = "CAB",
            IsActive = true,
            PasswordHash = PasswordHasher.Hash("Admin123!")
        });
    }

    db.SaveChanges();
}

public partial class Program;
