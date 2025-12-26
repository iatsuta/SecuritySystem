using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public interface ITestingUserAuthenticationService : IRawUserAuthenticationService, IImpersonateService
{
    void SetUser(UserCredential? customUserCredential);

    void Reset();
}