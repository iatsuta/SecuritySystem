using SecuritySystem;

namespace ExampleApp.Application;

public static class ExampleRoles
{
    public static SecurityRole TestManager { get; } = new (nameof(TestManager));

    public static SecurityRole BuManager { get; } = new(nameof(BuManager));

    public static SecurityRole OtherRole { get; } = new(nameof(OtherRole));
}