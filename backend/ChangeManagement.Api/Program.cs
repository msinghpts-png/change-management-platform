using ChangeManagement.Api.Data;
using ChangeManagement.Api.Repositories;
using ChangeManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=ChangeManagementDB;Trusted_Connection=true;TrustServerCertificate=true;";
builder.Services.AddDbContext<ChangeManagementDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddControllers();
builder.Services.AddScoped<IChangeStatusValidator, ChangeStatusValidator>();
builder.Services.AddScoped<IChangeRepository, ChangeRepository>();
builder.Services.AddScoped<IChangeService, ChangeService>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IChangeAttachmentRepository, ChangeAttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

var app = builder.Build();

Directory.CreateDirectory("/app/uploads");

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    var result = await initializer.InitializeAsync();

    logger.LogInformation(
        "Database startup completed. DatabaseExisted={DatabaseExisted}, Seeded={Seeded}",
        result.DatabaseExisted,
        result.Seeded);
}

app.MapControllers();

app.Run();

public partial class Program
{
}
