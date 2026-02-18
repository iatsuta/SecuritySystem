using System.Collections.Immutable;

namespace SecuritySystem.ExternalSystem.Management;

public abstract record PermissionData
{
    public abstract Type PermissionType { get; }
}

public abstract record PermissionData<TPermission>(TPermission Permission) : PermissionData
{
    public override Type PermissionType { get; } = typeof(TPermission);
}

public record PermissionData<TPermission, TPermissionRestriction>(TPermission Permission, ImmutableArray<TPermissionRestriction> Restrictions)
    : PermissionData<TPermission>(Permission)
{
    public PermissionData(TPermission permission, IEnumerable<TPermissionRestriction> restrictions)
        : this(permission, [..restrictions])
    {
    }
}