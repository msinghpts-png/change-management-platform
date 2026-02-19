using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChangeManagement.Api.DTOs.Auth;

namespace ChangeManagement.Api.Tests.Infrastructure;

internal static class AuthTestHelper
{
    public static Task AuthenticateAsAdminAsync(HttpClient client) =>
        AuthenticateAsync(client, "admin@local", "Admin123!");

    public static Task AuthenticateAsCabAsync(HttpClient client) =>
        AuthenticateAsync(client, "cab@local", "Admin123!");

    public static Task AuthenticateAsExecutorAsync(HttpClient client) =>
        AuthenticateAsync(client, "executor@local", "Admin123!");

    private static async Task AuthenticateAsync(HttpClient client, string upn, string password)
    {
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Upn = upn,
            Password = password
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Test login failed for {upn}. Status: {(int)response.StatusCode} Body: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        if (string.IsNullOrWhiteSpace(payload?.Token))
        {
            throw new InvalidOperationException($"Test login succeeded for {upn} but token was missing.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.Token);
    }
}
