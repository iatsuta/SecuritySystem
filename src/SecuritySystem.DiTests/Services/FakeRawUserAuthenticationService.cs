using SecuritySystem.Services;

namespace SecuritySystem.DiTests.Services;

public class FakeRawUserAuthenticationService : IRawUserAuthenticationService
{
    public string GetUserName()
    {
        throw new NotImplementedException();
    }
}