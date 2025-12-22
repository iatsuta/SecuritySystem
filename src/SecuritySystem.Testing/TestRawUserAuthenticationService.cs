using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public class TestRawUserAuthenticationService : IRawUserAuthenticationService
{
    public string CurrentUserName { get; set; } = "TestRootUser";

    public string GetUserName() => this.CurrentUserName;
}