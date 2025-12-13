using SecuritySystem.Services;

namespace ExampleApp.IntegrationTests.Services;

public class TestRawUserAuthenticationService : IRawUserAuthenticationService
{
    public string CurrentUserName { get; set; } = "TestRootUser";

    public string GetUserName() => this.CurrentUserName;
}