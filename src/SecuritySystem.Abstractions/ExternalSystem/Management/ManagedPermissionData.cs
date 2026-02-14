namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPermissionData
{
    public required SecurityRole SecurityRole { get; init; }

    public PermissionPeriod Period { get; init; } = PermissionPeriod.Eternity;

    public string Comment { get; init; } = "";

    public IReadOnlyDictionary<Type, Array> Restrictions { get; init; } = new Dictionary<Type, Array>();

    public static implicit operator ManagedPermissionData(SecurityRole securityRole) => new() { SecurityRole = securityRole };
}