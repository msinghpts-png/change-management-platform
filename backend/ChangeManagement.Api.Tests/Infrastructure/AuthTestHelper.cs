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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName, "enabled");

        if (client.DefaultRequestHeaders.Contains(TestAuthHandler.UserIdHeader))
            client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        if (client.DefaultRequestHeaders.Contains(TestAuthHandler.UserNameHeader))
            client.DefaultRequestHeaders.Remove(TestAuthHandler.UserNameHeader);
        if (client.DefaultRequestHeaders.Contains(TestAuthHandler.RoleHeader))
            client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);

        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeader, userName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);

        return Task.CompletedTask;
    }
}
