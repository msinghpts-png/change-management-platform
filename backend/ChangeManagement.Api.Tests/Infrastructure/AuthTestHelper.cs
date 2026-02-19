using System.Net.Http.Headers;

namespace ChangeManagement.Api.Tests.Infrastructure;

internal static class AuthTestHelper
{
    public static Task AuthenticateAsAdminAsync(HttpClient client) =>
        AuthenticateAsync(client, "11111111-1111-1111-1111-111111111111", "admin@local", "Admin");

    public static Task AuthenticateAsCabAsync(HttpClient client) =>
        AuthenticateAsync(client, "22222222-2222-2222-2222-222222222222", "cab@local", "CAB");

    public static Task AuthenticateAsExecutorAsync(HttpClient client) =>
        AuthenticateAsync(client, "33333333-3333-3333-3333-333333333333", "executor@local", "Executor");

    private static Task AuthenticateAsync(HttpClient client, string userId, string userName, string role)
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
