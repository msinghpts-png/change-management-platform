using System.Net.Http.Json;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class AuditEventTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuditEventTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateChange_WritesAuditEvent()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);

        var payload = new
        {
            Title = "audit-test",
            Description = "desc",
            ChangeTypeId = 1,
            PriorityId = 1,
            RiskLevelId = 1,
            RequestedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        var response = await _client.PostAsJsonAsync("/api/changes", payload);
        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChangeManagementDbContext>();
        Assert.True(db.AuditEvents.Any());
    }
}
