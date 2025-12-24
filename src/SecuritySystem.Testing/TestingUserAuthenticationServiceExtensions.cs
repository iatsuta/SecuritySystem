using SecuritySystem.Credential;

namespace SecuritySystem.Testing;

public static class TestingUserAuthenticationServiceExtensions
{
    public static async Task WithImpersonateAsync(
        this ITestingUserAuthenticationService service,
        UserCredential customUserCredential,
        Func<Task> action)
    {
        await service.WithImpersonateAsync(
            customUserCredential,
            async () =>
            {
                await action();
                return default(object);
            });
    }
}