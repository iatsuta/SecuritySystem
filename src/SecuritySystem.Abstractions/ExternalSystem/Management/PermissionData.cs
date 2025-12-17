namespace SecuritySystem.ExternalSystem.Management;

public abstract record PermissionData
{
    public abstract Type PermissionTypeType { get; }
}

public abstract record PermissionData<TPermission>(TPermission Permission) : PermissionData
{
    public override Type PermissionTypeType { get; } = typeof(TPermission);
}

public record PermissionData<TPermission, TPermissionRestriction>(TPermission Permission, IReadOnlyList<TPermissionRestriction> Restrictions) : PermissionData<TPermission>(Permission);