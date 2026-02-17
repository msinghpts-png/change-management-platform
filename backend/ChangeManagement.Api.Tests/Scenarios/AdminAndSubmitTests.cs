using System.Net;
using System.Net.Http.Json;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class AdminAndSubmitTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminAndSubmitTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DemoDataEndpoint_RequiresAdminAuthentication()
    {
        var response = await _client.PostAsync("/api/admin/demo-data", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitDraft_DoesNotRequireAttachments()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/changes", new
        {
            Title = "submit-no-attachment",
            Description = "impact description",
            BackoutPlan = "rollback",
            ChangeTypeId = 1,
            PriorityId = 2,
            RiskLevelId = 2,
            ImpactTypeId = 2,
            RequestedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PlannedStart = DateTime.UtcNow.AddDays(1)
        });
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var changeId = Guid.Parse(created!["changeRequestId"].ToString()!);

        var submitResponse = await _client.PostAsync($"/api/changes/{changeId}/submit", content: null);
        submitResponse.EnsureSuccessStatusCode();
    }
}
