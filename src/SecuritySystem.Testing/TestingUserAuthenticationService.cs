using System.Collections.Concurrent;

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public class TestingUserAuthenticationService(
    ITestingEvaluator<IUserCredentialNameResolver> credentialNameResolverEvaluator,
    TestRootUserInfo testRootUserInfo)
    : ITestingUserAuthenticationService
{
    private readonly ConcurrentDictionary<UserCredential, string> credCache = [];

    public UserCredential? CustomUserCredential { get; set; }

    public string GetUserName() =>
        this.CustomUserCredential == null
            ? testRootUserInfo.Name
            : this.credCache.GetOrAdd(this.CustomUserCredential, _ => this.CustomUserCredential switch
            {
                UserCredential.NamedUserCredential { Name: var name } => name,

                _ => credentialNameResolverEvaluator.EvaluateAsync(TestingScopeMode.Read, async resolver => resolver.GetUserName(this.CustomUserCredential))
                    .GetAwaiter().GetResult()
            });

    public async Task<T> WithImpersonateAsync<T>(UserCredential customUserCredential, Func<Task<T>> func)
    {
        var prev = this.CustomUserCredential;

        this.CustomUserCredential = customUserCredential;

        try
        {
            return await func();
        }
        finally
        {
            this.CustomUserCredential = prev;
        }
    }
}