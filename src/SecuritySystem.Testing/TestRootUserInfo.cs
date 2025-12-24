namespace SecuritySystem.Testing;

public record TestRootUserInfo(string Name)
{
    public static TestRootUserInfo Default { get; } = new("TestRootUser");
}