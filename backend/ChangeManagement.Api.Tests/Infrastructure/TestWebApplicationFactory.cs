using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeManagement.Api.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChangeManagementDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            services.AddDbContext<ChangeManagementDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
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
                Role = "Manager",
                IsActive = true
            });
            db.SaveChanges();
        }
    }
}
