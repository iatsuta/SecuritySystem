using SecuritySystem.Services;

namespace ExampleApp.IntegrationTests.Services;

public class TestRawUserAuthenticationService : IRawUserAuthenticationService
{
    public string GetUserName() => "TestRootUser";
}