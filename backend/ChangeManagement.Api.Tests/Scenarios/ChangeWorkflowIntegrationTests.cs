using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class ChangeWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CabUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly HttpClient _client;

    public ChangeWorkflowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Submit_ApproveAllStrategy_WorksEndToEnd()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var changeId = await CreateChangeAsync("approve-all-change");

        var submit = await _client.PostAsJsonAsync($"/api/changes/{changeId}/submit", new
        {
            approvalStrategy = "All",
            approverUserIds = new[] { AdminUserId, CabUserId },
            reason = "submit for all approvals"
        });
        submit.EnsureSuccessStatusCode();

        var submitted = await submit.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(submitted);
        Assert.Equal(3, submitted!.StatusId);
        Assert.True(submitted.ApprovalRequired);
        Assert.Equal("All", submitted.ApprovalStrategy);
        Assert.Equal(2, submitted.ApproverUserIds.Distinct().Count());

        var approveByAdmin = await _client.PostAsJsonAsync($"/api/changes/{changeId}/approve", new { comments = "admin approved" });
        approveByAdmin.EnsureSuccessStatusCode();

        var afterFirstApproval = await approveByAdmin.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(afterFirstApproval);
        Assert.Equal(3, afterFirstApproval!.StatusId);

        await AuthTestHelper.AuthenticateAsCabAsync(_client);
        var approveByCab = await _client.PostAsJsonAsync($"/api/changes/{changeId}/approve", new { comments = "cab approved" });
        approveByCab.EnsureSuccessStatusCode();

        var approved = await approveByCab.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(approved);
        Assert.Equal(4, approved!.StatusId);
    }

    [Fact]
    public async Task Reject_TransitionsToRejected()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var changeId = await CreateChangeAsync("reject-change");

        var submit = await _client.PostAsJsonAsync($"/api/changes/{changeId}/submit", new
        {
            approvalStrategy = "All",
            approverUserIds = new[] { CabUserId }
        });
        submit.EnsureSuccessStatusCode();

        await AuthTestHelper.AuthenticateAsCabAsync(_client);
        var reject = await _client.PostAsJsonAsync($"/api/changes/{changeId}/reject", new { comments = "rejected by cab" });
        reject.EnsureSuccessStatusCode();

        var rejected = await reject.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(rejected);
        Assert.Equal(5, rejected!.StatusId);
    }

    [Fact]
    public async Task Upload_Download_And_SoftDelete_HidesFromList()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var changeId = await CreateChangeAsync("attachment-and-delete");

        using var form = new MultipartFormDataContent();
        var bytes = Encoding.UTF8.GetBytes("integration-attachment");
        using var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(file, "file", "integration.txt");

        var upload = await _client.PostAsync($"/api/changes/{changeId}/attachments", form);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);

        using var uploadJson = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var attachmentId = uploadJson.RootElement.GetProperty("id").GetGuid();

        var download = await _client.GetAsync($"/api/changes/{changeId}/attachments/{attachmentId}/download");
        download.EnsureSuccessStatusCode();
        var downloaded = await download.Content.ReadAsByteArrayAsync();
        Assert.Equal(bytes, downloaded);

        var delete = await _client.DeleteAsync($"/api/changes/{changeId}?reason=integration-test-delete");
        delete.EnsureSuccessStatusCode();

        var list = await _client.GetFromJsonAsync<List<ChangeRequestResponse>>("/api/changes");
        Assert.NotNull(list);
        Assert.DoesNotContain(list!, x => x.ChangeRequestId == changeId || x.Id == changeId);
    }

    [Fact]
    public async Task DuplicateApprovers_ArePrevented_OnSubmit()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);
        var changeId = await CreateChangeAsync("duplicate-approver");

        var submit = await _client.PostAsJsonAsync($"/api/changes/{changeId}/submit", new
        {
            approvalStrategy = "All",
            approverUserIds = new[] { CabUserId, CabUserId, CabUserId }
        });
        submit.EnsureSuccessStatusCode();

        var submitted = await submit.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(submitted);
        Assert.Single(submitted!.ApproverUserIds.Distinct());
        Assert.Equal(CabUserId, submitted.ApproverUserIds.Single());
    }

    private async Task<Guid> CreateChangeAsync(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/changes", new
        {
            title,
            description = "integration-test-description",
            changeTypeId = 1,
            priorityId = 2,
            riskLevelId = 2,
            requestedByUserId = AdminUserId,
            plannedStart = DateTime.UtcNow.AddHours(1),
            backoutPlan = "integration-test-backout"
        });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("changeRequestId").GetGuid();
    }

    private sealed class ChangeRequestResponse
    {
        public Guid Id { get; set; }
        public Guid ChangeRequestId { get; set; }
        public int StatusId { get; set; }
        public bool ApprovalRequired { get; set; }
        public string? ApprovalStrategy { get; set; }
        public List<Guid> ApproverUserIds { get; set; } = new();
    }
}
