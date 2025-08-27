using SecuritySystem.Services;

namespace ExampleApp.IntegrationTests;

public class TestRawUserAuthenticationService : IRawUserAuthenticationService
{
    public string GetUserName() => "TestRootUser";
}