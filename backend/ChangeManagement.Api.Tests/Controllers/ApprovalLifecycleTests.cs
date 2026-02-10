using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChangeManagement.Api.Tests.Controllers;

public class ApprovalLifecycleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApprovalLifecycleTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Draft_Submit_Moves_To_PendingApproval()
    {
        var change = await CreateChangeAsync();

        var submitResponse = await _client.PostAsync($"/api/changes/{change.Id}/submit", null);
        submitResponse.EnsureSuccessStatusCode();

        var updated = await submitResponse.Content.ReadFromJsonAsync<ChangeRequestDto>();

        Assert.NotNull(updated);
        Assert.Equal("PendingApproval", updated!.Status);
    }

    [Fact]
    public async Task PendingApproval_Approved_Moves_To_Approved()
    {
        var change = await CreateChangeAsync();
        await _client.PostAsync($"/api/changes/{change.Id}/submit", null);

        var approval = await CreateApprovalAsync(change.Id);
        var decision = new ApprovalDecisionDto { Status = "Approved", Comment = "Looks good." };

        var decisionResponse = await _client.PostAsJsonAsync($"/api/changes/{change.Id}/approvals/{approval.Id}/decision", decision);
        decisionResponse.EnsureSuccessStatusCode();

        var refreshed = await _client.GetFromJsonAsync<ChangeRequestDto>($"/api/changes/{change.Id}");
        Assert.NotNull(refreshed);
        Assert.Equal("Approved", refreshed!.Status);
    }

    [Fact]
    public async Task PendingApproval_Rejected_Moves_To_Rejected()
    {
        var change = await CreateChangeAsync();
        await _client.PostAsync($"/api/changes/{change.Id}/submit", null);

        var approval = await CreateApprovalAsync(change.Id);
        var decision = new ApprovalDecisionDto { Status = "Rejected", Comment = "Missing details." };

        var decisionResponse = await _client.PostAsJsonAsync($"/api/changes/{change.Id}/approvals/{approval.Id}/decision", decision);
        decisionResponse.EnsureSuccessStatusCode();

        var refreshed = await _client.GetFromJsonAsync<ChangeRequestDto>($"/api/changes/{change.Id}");
        Assert.NotNull(refreshed);
        Assert.Equal("Rejected", refreshed!.Status);
    }

    [Fact]
    public async Task Invalid_Submit_Transition_Is_Blocked()
    {
        var change = await CreateChangeAsync();
        await _client.PostAsync($"/api/changes/{change.Id}/submit", null);

        var secondSubmit = await _client.PostAsync($"/api/changes/{change.Id}/submit", null);

        Assert.Equal(HttpStatusCode.BadRequest, secondSubmit.StatusCode);
    }

    private async Task<ChangeRequestDto> CreateChangeAsync()
    {
        var payload = new ChangeCreateDto
        {
            Title = "Test change",
            Description = "Test description"
        };

        var response = await _client.PostAsJsonAsync("/api/changes", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ChangeRequestDto>();
        Assert.NotNull(created);

        return created!;
    }

    private async Task<ApprovalDto> CreateApprovalAsync(Guid changeId)
    {
        var payload = new ApprovalCreateDto
        {
            Approver = "approver@example.com",
            Comment = "Reviewing"
        };

        var response = await _client.PostAsJsonAsync($"/api/changes/{changeId}/approvals", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ApprovalDto>();
        Assert.NotNull(created);

        return created!;
    }

    private sealed class ChangeCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private sealed class ChangeRequestDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class ApprovalCreateDto
    {
        public string Approver { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }

    private sealed class ApprovalDecisionDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }

    private sealed class ApprovalDto
    {
        public Guid Id { get; set; }
    }
}
