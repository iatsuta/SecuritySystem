using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public interface IImpersonateService
{
    Task<T> WithImpersonateAsync<T>(UserCredential customUserCredential, Func<Task<T>> func);
}