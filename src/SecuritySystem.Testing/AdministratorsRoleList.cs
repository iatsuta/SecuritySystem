namespace SecuritySystem.Testing;

public record AdministratorsRoleList(IReadOnlyList<SecurityRole> Roles)
{
    public static AdministratorsRoleList Default { get; } = new([SecurityRole.Administrator, SecurityRole.SystemIntegration]);
}