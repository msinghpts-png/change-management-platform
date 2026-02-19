using System.Net;
using System.Net.Http.Json;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class WorkflowEngineTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WorkflowEngineTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DraftSave_AllowsMissingRequiredFields()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var response = await _client.PostAsJsonAsync("/api/changes", new { Title = "draft-minimal" });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Submit_EnforcesRequiredFields()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var createResponse = await _client.PostAsJsonAsync("/api/changes", new { Title = "submit-invalid" });
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeCreatedResponse>();
        var changeId = created!.ChangeRequestId;

        var submit = await _client.PostAsJsonAsync($"/api/changes/{changeId}/submit", new { approverUserIds = new[] { "22222222-2222-2222-2222-222222222222" } });
        Assert.Equal(HttpStatusCode.BadRequest, submit.StatusCode);
    }

    [Fact]
    public async Task NonCab_CannotApprove()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var createResponse = await _client.PostAsJsonAsync("/api/changes", new
        {
            Title = "approval-role-check",
            Description = "desc",
            BackoutPlan = "rollback",
            ChangeTypeId = 2,
            PriorityId = 2,
            RiskLevelId = 2,
            PlannedStart = DateTime.UtcNow.AddDays(1)
        });
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeCreatedResponse>();
        var createdId = created!.ChangeRequestId;

        await AuthTestHelper.AuthenticateAsExecutorAsync(_client);
        var response = await _client.PostAsJsonAsync($"/api/changes/{createdId}/approve", new { comments = "no" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record ChangeCreatedResponse(Guid ChangeRequestId);
}
