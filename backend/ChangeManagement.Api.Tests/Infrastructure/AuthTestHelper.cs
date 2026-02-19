using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChangeManagement.Api.Tests.Infrastructure;

internal static class AuthTestHelper
{
    public static Task AuthenticateAsAdminAsync(HttpClient client) => AuthenticateAsync(client, "admin@local");

    public static Task AuthenticateAsCabAsync(HttpClient client) => AuthenticateAsync(client, "cab@local");

    public static Task AuthenticateAsExecutorAsync(HttpClient client) => AuthenticateAsync(client, "executor@local");

    private static async Task AuthenticateAsync(HttpClient client, string upn)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { upn, password = "Admin123!" });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<LoginPayload>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.Token);
    }

    private sealed class LoginPayload
    {
        public string Token { get; set; } = string.Empty;
    }
}
