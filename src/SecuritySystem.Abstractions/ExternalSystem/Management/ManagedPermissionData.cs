using System.Collections.Immutable;

namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPermissionData
{
    public required SecurityRole SecurityRole { get; init; }

    public PermissionPeriod Period { get; init; } = PermissionPeriod.Eternity;

    public string Comment { get; init; } = "";

    public ImmutableDictionary<Type, Array> Restrictions { get; init; } = [];

    public ImmutableDictionary<string, object> ExtendedData { get; init; } = [];

    public static implicit operator ManagedPermissionData(SecurityRole securityRole) => new() { SecurityRole = securityRole };
}