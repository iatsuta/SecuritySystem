using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public abstract class ImpersonateUserAuthenticationService(IUserCredentialNameResolver userCredentialNameResolver)
    : IRawUserAuthenticationService, IImpersonateService
{
    public string GetUserName() => this.CustomUserCredential == null
        ? this.GetPureUserName()
        : userCredentialNameResolver.GetUserName(this.CustomUserCredential);

    protected abstract string GetPureUserName();

    public UserCredential? CustomUserCredential { get; private set; }

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