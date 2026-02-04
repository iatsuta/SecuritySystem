namespace SecuritySystem.Services;

public class FixedRawUserAuthenticationService(string userName) : IRawUserAuthenticationService
{
    public string GetUserName() => userName;

    public static FixedRawUserAuthenticationService CurrentMachine { get; } = new(
        $"{Environment.MachineName}\\{Environment.UserName}");
}