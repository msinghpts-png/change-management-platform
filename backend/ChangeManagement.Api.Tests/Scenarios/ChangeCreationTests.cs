using System.Net;
using System.Net.Http.Json;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class ChangeCreationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChangeCreationTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task DraftCreate_ReturnsIdentifier()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);

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

        var body = await response.Content.ReadFromJsonAsync<ChangeCreatedResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.ChangeRequestId);
    }

    [Fact]
    public async Task DraftUpdate_DoesNotDuplicate()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/changes", new
        {
            Title = "Draft original",
            ChangeTypeId = 2,
            PriorityId = 2,
            RiskLevelId = 2
        });
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeCreatedResponse>();
        var changeId = created!.ChangeRequestId;

        var updateResponse = await _client.PutAsJsonAsync($"/api/changes/{changeId}", new
        {
            Title = "Draft updated",
            Description = "updated",
            ChangeTypeId = 2,
            PriorityId = 2,
            RiskLevelId = 2,
            StatusId = 1,
            UpdatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111")
        });
        updateResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.GetAsync("/api/changes");
        listResponse.EnsureSuccessStatusCode();
        var changes = await listResponse.Content.ReadFromJsonAsync<List<ChangeSummaryResponse>>();

        Assert.Equal(1, changes!.Count(x => x.ChangeRequestId == changeId));
    }

    [Fact]
    public async Task GetChangeById_Returns200_ForExistingDraft()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/changes", new
        {
            Title = "Get by id",
            ChangeTypeId = 2,
            PriorityId = 2,
            RiskLevelId = 2
        });
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeCreatedResponse>();
        var response = await _client.GetAsync($"/api/changes/{created!.ChangeRequestId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetChangeById_Returns404_WhenMissing()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var response = await _client.GetAsync($"/api/changes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record ChangeCreatedResponse(Guid ChangeRequestId);

    private sealed class ChangeSummaryResponse
    {
        public Guid ChangeRequestId { get; set; }
    }
}
