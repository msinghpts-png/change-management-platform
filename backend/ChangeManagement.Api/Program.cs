using ChangeManagement.Api.Data;
using ChangeManagement.Api.Repositories;
using ChangeManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
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

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program
{
}
