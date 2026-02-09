using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChangeManagement.Api.Tests.Controllers;

public class ChangesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ChangesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Changes_Returns_OK()
    {
        var response = await _client.GetAsync("/api/changes");
        response.EnsureSuccessStatusCode();
    }
}
