using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Tests.Infrastructure;
using Xunit;

namespace ChangeManagement.Api.Tests.Scenarios;

public class AttachmentUploadTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public AttachmentUploadTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task UploadAttachment_ReturnsCreated_And_DownloadsContent()
    {
        await AuthTestHelper.AuthenticateAsAdminAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/changes", new
        {
            Title = "attachment-test",
            Description = "desc",
            ChangeTypeId = 1,
            PriorityId = 1,
            RiskLevelId = 1,
            RequestedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var changeId = Guid.Parse(created!["changeRequestId"].ToString()!);

        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent("hello"u8.ToArray());
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(file, "file", "note.txt");

        var uploadResponse = await _client.PostAsync($"/api/changes/{changeId}/attachments", form);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<AttachmentDto>();
        Assert.NotNull(uploaded);
        Assert.Equal(changeId, uploaded!.ChangeRequestId);
        Assert.Contains(changeId.ToString(), uploaded.FilePath, StringComparison.OrdinalIgnoreCase);

        var downloadResponse = await _client.GetAsync($"/api/changes/{changeId}/attachments/{uploaded.Id}/download");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        Assert.True(downloadResponse.Content.Headers.ContentLength > 0);
    }
}
