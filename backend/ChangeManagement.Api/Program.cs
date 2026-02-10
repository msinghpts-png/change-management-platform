using ChangeManagement.Api.Repositories;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IChangeStatusValidator, ChangeStatusValidator>();
builder.Services.AddSingleton<IChangeRepository, ChangeRepository>();
builder.Services.AddSingleton<IChangeService, ChangeService>();
builder.Services.AddSingleton<IApprovalRepository, ApprovalRepository>();
builder.Services.AddSingleton<IApprovalService, ApprovalService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program
{
}
