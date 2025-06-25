using SecuritySystem.Services;

namespace SecuritySystem.DiTests;

public class TestRawUserAuthenticationService : IRawUserAuthenticationService
{
    public string GetUserName() => "TestRootUser";
}