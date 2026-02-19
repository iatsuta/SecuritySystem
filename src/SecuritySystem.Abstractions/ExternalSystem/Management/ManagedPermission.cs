using System.Collections.Immutable;

namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPermission
{
    public required SecurityRole SecurityRole { get; init; }

    public SecurityIdentity Identity { get; init; } = SecurityIdentity.Default;

    public bool ForceApplyIdentity { get; init; }

    public bool IsVirtual { get; init; }

    public PermissionPeriod Period { get; init; } = PermissionPeriod.Eternity;

    public string Comment { get; init; } = "";

    public SecurityIdentity DelegatedFrom { get; init; } = SecurityIdentity.Default;

    public ImmutableDictionary<Type, Array> Restrictions { get; init; } = [];

    public ImmutableDictionary<string, object> ExtendedData { get; init; } = [];


    public static implicit operator ManagedPermission(SecurityRole securityRole) => new() { SecurityRole = securityRole };

    public ManagedPermission WithExtendedData(string key, object value)
    {
        var newExtendedData = this.ExtendedData.ToDictionary();

        newExtendedData[key] = value;

        return this with { ExtendedData = newExtendedData.ToImmutableDictionary() };
    }
}