using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChangeManagement.Api.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ChangeManagementTests_{Guid.NewGuid():N}";
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.Testing.json", optional: true);
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SkipDatabaseInitialization"] = "true",
                ["UseInMemoryDatabase"] = "true"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ChangeManagementDbContext>));
            services.RemoveAll(typeof(ChangeManagementDbContext));

            services.AddDbContext<ChangeManagementDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName, _databaseRoot));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    private static void Seed(ChangeManagementDbContext db)
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (!db.Users.Any(u => u.UserId == userId))
        {
            db.Users.Add(new User
            {
                UserId = userId,
                Upn = "admin@local",
                DisplayName = "Tester",
                Role = "Admin",
                IsActive = true,
                PasswordHash = PasswordHasher.Hash("Admin123!")
            });
            db.SaveChanges();
        }

        if (!db.ChangeTemplates.Any())
        {
            db.ChangeTemplates.Add(new ChangeTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = "Test Template",
                Description = "Template for tests",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
            db.SaveChanges();
        }
    }
}
