using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public interface ITestingUserAuthenticationService : IRawUserAuthenticationService
{
    void SetUser(UserCredential? customUserCredential);

    void Reset();

    Task<T> WithImpersonateAsync<T>(UserCredential customUserCredential, Func<Task<T>> func);
}