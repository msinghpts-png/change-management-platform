using ChangeManagement.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeManagement.Api.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChangeManagementDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ChangeManagementDbContext>(options =>
                options.UseInMemoryDatabase($"ChangeManagementTests-{Guid.NewGuid()}"));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    private static void Seed(ChangeManagementDbContext db)
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (!db.Users.Any(u => u.UserId == userId))
        {
            db.Users.Add(new ChangeManagement.Api.Domain.Entities.User
            {
                UserId = userId,
                Upn = "tester@example.com",
                DisplayName = "Tester",
                Role = "Manager",
                IsActive = true
            });
            db.SaveChanges();
        }
    }
}
