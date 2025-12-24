using CommonFramework.DictionaryCache;

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public class TestingUserAuthenticationService(
    ITestingEvaluator<IUserCredentialNameResolver> credentialNameResolverEvaluator,
    TestRootUserInfo testRootUserInfo)
    : ITestingUserAuthenticationService
{
    private readonly IDictionaryCache<UserCredential, string> credCache = new DictionaryCache<UserCredential, string>(
        userCredential =>
        {
            return userCredential switch
            {
                UserCredential.NamedUserCredential { Name: var name } => name,

                _ => credentialNameResolverEvaluator.EvaluateAsync(async resolver => resolver.GetUserName(userCredential)).GetAwaiter().GetResult()
            };
        });

    private string DefaultTestUserName => testRootUserInfo.Name;

    public UserCredential? CustomUserCredential { get; internal set; }

    public void SetUser(UserCredential? customUserCredential) =>
        this.CustomUserCredential = customUserCredential ?? this.DefaultTestUserName;

    public void Reset() => this.CustomUserCredential = this.DefaultTestUserName;

    public string GetUserName() =>
        this.CustomUserCredential == null ? this.DefaultTestUserName : this.credCache[this.CustomUserCredential];

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