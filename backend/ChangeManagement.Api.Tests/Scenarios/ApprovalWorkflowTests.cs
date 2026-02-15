using System.Net.Http.Json;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class ApprovalWorkflowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ApprovalWorkflowTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task ApprovalDecision_CanBeRecorded()
    {
        var change = await (await _client.PostAsJsonAsync("/api/changes", new
        {
            Title = "approval-test",
            Description = "desc",
            ChangeTypeId = 1,
            PriorityId = 1,
            RiskLevelId = 1,
            RequestedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        })).Content.ReadFromJsonAsync<dynamic>();

        var changeId = (Guid)change!.changeRequestId;
        var approvalResp = await _client.PostAsJsonAsync($"/api/changes/{changeId}/approvals", new { ApproverUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Comments = "ok" });
        approvalResp.EnsureSuccessStatusCode();
    }
}
