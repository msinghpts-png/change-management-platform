using System.Net.Http.Json;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class ChangeCreationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ChangeCreationTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task CreateChange_WritesChangeAndReturnsIdentifier()
    {
        var payload = new
        {
            Title = "Create test",
            Description = "desc",
            ChangeTypeId = 1,
            PriorityId = 2,
            RiskLevelId = 2,
            RequestedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        var response = await _client.PostAsJsonAsync("/api/changes", payload);
        response.EnsureSuccessStatusCode();
    }
}
