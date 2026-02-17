using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChangeManagement.Api.Tests.Infrastructure;

internal static class AuthTestHelper
{
    public static async Task AuthenticateAsAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            upn = "admin@local",
            password = "Admin123!"
        });
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginPayload>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.Token);
    }

    private sealed class LoginPayload
    {
        public string Token { get; set; } = string.Empty;
    }
}
