using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public interface ITestingUserAuthenticationService : IRawUserAuthenticationService, IImpersonateService
{
    UserCredential? CustomUserCredential { get; set; }
}