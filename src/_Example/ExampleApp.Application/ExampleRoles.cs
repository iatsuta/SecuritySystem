using SecuritySystem;

namespace ExampleApp.Application;

public static class ExampleRoles
{
    public static SecurityRole TestManager { get; } = new SecurityRole(nameof(TestManager));
}