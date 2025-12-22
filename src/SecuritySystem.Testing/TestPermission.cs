namespace SecuritySystem.Testing;

public record TestPermission(SecurityRole SecurityRole)
{
    public IReadOnlyDictionary<Type, Array> Restrictions { get; init; } = new Dictionary<Type, Array>();

    public PermissionPeriod Period { get; init; } = PermissionPeriod.Eternity;

    public static implicit operator TestPermission(SecurityRole securityRole)
    {
        return new TestPermission(securityRole);
    }
}