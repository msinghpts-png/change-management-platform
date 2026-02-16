using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChangeManagement.Api.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SkipDatabaseInitialization"] = "true"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ChangeManagementDbContext>));
            services.RemoveAll(typeof(ChangeManagementDbContext));

            services.AddDbContext<ChangeManagementDbContext>(options =>
                options.UseInMemoryDatabase($"ChangeManagementTests_{Guid.NewGuid():N}"));

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
                Upn = "tester@example.com",
                DisplayName = "Tester",
                Role = "Admin",
                IsActive = true,
                PasswordHash = string.Empty
            });
            db.SaveChanges();
        }
    }
}
