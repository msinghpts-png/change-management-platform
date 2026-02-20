using System.Net.Http.Headers;

namespace ChangeManagement.Api.Tests.Infrastructure;

internal static class AuthTestHelper
{
    public static Task AuthenticateAsAdminAsync(HttpClient client) =>
        AuthenticateAsync(client, "admin@local", "Admin123!");

    public static Task AuthenticateAsCabAsync(HttpClient client) =>
        AuthenticateAsync(client, "cab@local", "Admin123!");

    public static Task AuthenticateAsExecutorAsync(HttpClient client) =>
        AuthenticateAsync(client, "executor@local", "Admin123!");

    private static Task AuthenticateAsync(HttpClient client, string upn, string password)
    {
        _ = password;

        var (userId, userName, role) = upn.ToLowerInvariant() switch
        {
            "cab@local" => ("22222222-2222-2222-2222-222222222222", "Cab Tester", "CAB"),
            "executor@local" => ("33333333-3333-3333-3333-333333333333", "Executor Tester", "Executor"),
            _ => ("11111111-1111-1111-1111-111111111111", "Tester", "Admin")
        };

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
